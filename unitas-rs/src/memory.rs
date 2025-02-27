use std::{
    collections::HashMap,
    ffi::{CStr, CString},
    io, mem,
    path::PathBuf,
    ptr,
    sync::{LazyLock, Mutex},
};

struct Memory {
    dynamic_inst_ptr_start: usize,
    dynamic_inst_ptr: usize,
    dynamic_inst_mem_size: usize,
    page_size: usize,
}

const DYNAMIC_INST_MEM_SIZE: usize = 0x20;

const BITNESS: u32 = (mem::size_of::<usize>() * 8) as u32;

#[cfg(all(unix, any(target_arch = "x86_64", target_arch = "x86")))]
static MEMORY: LazyLock<Mutex<Memory>> = LazyLock::new(|| {
    use libc::*;

    let page_size = unsafe { sysconf(_SC_PAGESIZE) } as usize;
    let dynamic_inst_mem_size = DYNAMIC_INST_MEM_SIZE.div_ceil(page_size) * page_size;
    let alloc_ptr = unsafe {
        mmap(
            std::ptr::null_mut(),
            dynamic_inst_mem_size,
            PROT_READ | PROT_WRITE | PROT_EXEC,
            MAP_ANONYMOUS | MAP_PRIVATE,
            -1,
            0,
        )
    };
    if alloc_ptr == libc::MAP_FAILED {
        panic!(
            "failed to initialise memory: {}",
            io::Error::last_os_error()
        );
    }

    let alloc_ptr = alloc_ptr as usize;

    Memory {
        dynamic_inst_ptr_start: alloc_ptr,
        dynamic_inst_ptr: alloc_ptr,
        dynamic_inst_mem_size,
        page_size,
    }
    .into()
});

impl Memory {
    fn check_dynamic_inst_ptr(&self) {
        let used = self.dynamic_inst_ptr - self.dynamic_inst_ptr_start;
        if used > self.dynamic_inst_mem_size {
            panic!(
                "ran out of space for dynamically generated x86 instructions, used {used:x} bytes"
            );
        }
    }
}

/// Hooks function at point in memory, targetting call instructions that is 5 bytes or more in length
///
/// Use this hook if you need some hook installed in the middle of the function
///
/// # Args
/// - `hook_target`: address where the `hook` should be installed in
/// - `original_before_hook`: run original code that is replaced before hook callback
/// - `hook`: function to be called
///
/// # Panics
/// - Target instruction size is less than 5 bytes total
///
/// # Safety
/// - There are no checks if `hook_target` is a valid memory address
/// - If the hook somehow fails, it will crash the program
#[cfg(any(target_arch = "x86_64", target_arch = "x86"))]
pub unsafe fn hook_inject(
    hook_target: usize,
    original_before_hook: bool,
    hook: extern "C" fn(),
) -> io::Result<()> {
    use std::slice;

    use iced_x86::{Decoder, DecoderOptions, code_asm::*};

    let memory = unsafe { slice::from_raw_parts(hook_target as *const u8, 15) };
    let mut decoder = Decoder::new(BITNESS, memory, DecoderOptions::NO_INVALID_CHECK);
    decoder.set_ip(hook_target as u64);
    let target_inst = decoder.decode();
    let target_inst_branch = target_inst.is_jmp_short_or_near() | target_inst.is_jmp_far();
    if target_inst.len() < 5 {
        panic!("target instruction is less than 5 bytes");
    }

    // rewrite original jump to custom location
    let memory = &mut MEMORY.lock().unwrap();

    let mut assembler = CodeAssembler::new(BITNESS).expect("failed to create code assembler");
    assembler.jmp(memory.dynamic_inst_ptr as u64).unwrap();
    let inst = assembler
        .assemble(hook_target as u64)
        .expect("failed to assemble jmp to custom location");
    assert_eq!(inst.len(), 5);
    make_exec_mem_writable(hook_target, inst.len(), memory.page_size)?;
    unsafe { ptr::copy_nonoverlapping(inst.as_ptr(), hook_target as *mut u8, inst.len()) };
    restore_exec_mem_prot(hook_target, inst.len(), memory.page_size)?;

    // custom location instructions
    let mut assembler = CodeAssembler::new(BITNESS).expect("failed to create code assembler");

    if original_before_hook {
        if target_inst_branch {
            panic!(
                "you cannot call hook_inject with `original_before_hook` for a branching instruction"
            );
        }
        assembler
            .add_instruction(target_inst)
            .expect("failed to add original instruction to assembler");
    }

    assembler.pushfq().unwrap();
    assembler.push(rax).unwrap();
    assembler.push(rbx).unwrap();
    assembler.push(rcx).unwrap();
    assembler.push(rdx).unwrap();
    assembler.push(rsi).unwrap();
    assembler.push(rdi).unwrap();
    assembler.push(rbp).unwrap();
    assembler.push(r8).unwrap();
    assembler.push(r9).unwrap();
    assembler.push(r10).unwrap();
    assembler.push(r11).unwrap();
    assembler.push(r12).unwrap();
    assembler.push(r13).unwrap();
    assembler.push(r14).unwrap();
    assembler.push(r15).unwrap();

    assembler.push(rbp).unwrap(); // align sp
    assembler.mov(rbp, rsp).unwrap();
    assembler.and(rsp, -16i32).unwrap();

    assembler.mov(rax, hook as usize as u64).unwrap();
    assembler.call(rax).unwrap();

    assembler.mov(rsp, rbp).unwrap();
    assembler.pop(rbp).unwrap();

    assembler.pop(r15).unwrap();
    assembler.pop(r14).unwrap();
    assembler.pop(r13).unwrap();
    assembler.pop(r12).unwrap();
    assembler.pop(r11).unwrap();
    assembler.pop(r10).unwrap();
    assembler.pop(r9).unwrap();
    assembler.pop(r8).unwrap();
    assembler.pop(rbp).unwrap();
    assembler.pop(rdi).unwrap();
    assembler.pop(rsi).unwrap();
    assembler.pop(rdx).unwrap();
    assembler.pop(rcx).unwrap();
    assembler.pop(rbx).unwrap();
    assembler.pop(rax).unwrap();
    assembler.popfq().unwrap();

    if !original_before_hook {
        // if target_inst.is_jmp_short_or_near() {}
        assembler
            .add_instruction(target_inst)
            .expect("failed to add original instruction to assembler");
    }

    if !target_inst_branch {
        assembler
            .jmp((hook_target + target_inst.len()) as u64)
            .unwrap();
    }

    let instrs = assembler
        .assemble(memory.dynamic_inst_ptr as u64)
        .expect("failed to assemble custom assembly");

    unsafe {
        ptr::copy_nonoverlapping(
            instrs.as_ptr(),
            memory.dynamic_inst_ptr as *mut u8,
            instrs.len(),
        )
    };

    memory.dynamic_inst_ptr += instrs.len();
    memory.check_dynamic_inst_ptr();

    Ok(())
}

#[cfg(all(unix, any(target_arch = "x86_64", target_arch = "x86")))]
fn make_exec_mem_writable(addr: usize, len: usize, page_size: usize) -> io::Result<()> {
    use libc::*;

    let page_start = addr & !(page_size - 1);
    let aligned_len = ((addr + len + page_size - 1) & !(page_size - 1)) - page_start;
    if unsafe {
        mprotect(
            page_start as *mut libc::c_void,
            aligned_len,
            PROT_READ | PROT_WRITE | PROT_EXEC,
        )
    } != 0
    {
        return Err(io::Error::last_os_error());
    }

    Ok(())
}

#[cfg(unix)]
fn restore_exec_mem_prot(addr: usize, len: usize, page_size: usize) -> io::Result<()> {
    use libc::*;

    let page_start = addr & !(page_size - 1);
    let aligned_len = ((addr + len + page_size - 1) & !(page_size - 1)) - page_start;
    if unsafe {
        mprotect(
            page_start as *mut libc::c_void,
            aligned_len,
            PROT_READ | PROT_EXEC,
        )
    } != 0
    {
        return Err(io::Error::last_os_error());
    }

    Ok(())
}

pub struct MemoryMap {
    pub path: PathBuf,
    pub start: usize,
    pub end: usize,
}

impl MemoryMap {
    #[cfg(unix)]
    // as of now, it is represented by a hashmap with the filename, and address
    // should be enough
    pub fn read_proc_maps() -> io::Result<Vec<MemoryMap>> {
        use std::{fs::File, io::Read, path::Path};

        let mut maps: Vec<MemoryMap> = Vec::new();
        let mut file = File::open("/proc/self/maps")?;
        let mut content = String::new();
        file.read_to_string(&mut content)?;

        for line in content.lines() {
            let mut parts = line.split_whitespace();

            let Some(addr_range) = parts.next() else {
                continue;
            };

            let mut addr_range = addr_range.split('-');

            let Some(start_addr) = addr_range.next() else {
                continue;
            };

            let Ok(start_addr) = usize::from_str_radix(start_addr, 16) else {
                continue;
            };

            let Some(end_addr) = addr_range.next() else {
                continue;
            };

            let Ok(end_addr) = usize::from_str_radix(end_addr, 16) else {
                continue;
            };

            let mut parts = parts.skip(4);

            let Some(path) = parts.next() else {
                continue;
            };

            let file_name = Path::new(path);

            if let Some(mem) = maps.iter_mut().find(|m| m.path == file_name) {
                mem.end = end_addr;
                continue;
            }

            maps.push(MemoryMap {
                path: file_name.to_path_buf(),
                start: start_addr,
                end: end_addr,
            });
        }

        Ok(maps)
    }
}

#[cfg(unix)]
pub struct SymbolLookup {
    // value contains the handle, and HashMap for symbol and its offset
    handles: HashMap<CString, (*mut libc::c_void, HashMap<CString, usize>)>,
}

#[cfg(unix)]
impl SymbolLookup {
    pub fn new() -> Self {
        Self {
            handles: HashMap::new(),
        }
    }

    pub fn get_symbol_in_file(&mut self, file_name: &CStr, symbol: &CStr) -> usize {
        use libc::*;

        let (handle, symbol_map) = match self.handles.get_mut(file_name) {
            Some(handle) => handle,
            None => {
                let handle = unsafe { dlopen(file_name.as_ptr(), RTLD_NOW | RTLD_NOLOAD) };
                if handle.is_null() {
                    // TODO: error diagnostics
                    panic!("failed to open library with dlopen");
                }
                self.handles
                    .insert(file_name.to_owned(), (handle, HashMap::new()));
                self.handles.get_mut(file_name).unwrap()
            }
        };

        match symbol_map.get(symbol) {
            Some(addr) => *addr,
            None => {
                let symbol_ptr = unsafe { dlsym(*handle, symbol.as_ptr()) };
                if symbol_ptr.is_null() {
                    // TODO: error diagnostics
                    panic!("failed to obtain symbol with dlsym");
                }
                let symbol_ptr = symbol_ptr as usize;
                symbol_map.insert(symbol.to_owned(), symbol_ptr);
                symbol_ptr
            }
        }
    }
}

#[cfg(unix)]
impl Drop for SymbolLookup {
    fn drop(&mut self) {
        use libc::*;

        for (handle, _) in self.handles.values() {
            unsafe {
                // TODO: do i check for error?
                dlclose(*handle);
            }
        }
    }
}

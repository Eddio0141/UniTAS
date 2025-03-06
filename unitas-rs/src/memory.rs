use std::{
    collections::HashMap,
    ffi::{CStr, CString},
    io, mem,
    ops::Range,
    ptr,
    sync::{LazyLock, Mutex},
};

struct Memory {
    dynamic_inst_ptr_start: usize,
    dynamic_inst_ptr: usize,
    dynamic_inst_mem_size: usize,
}

const DYNAMIC_INST_MEM_SIZE: usize = 0x1000;

const BITNESS: u32 = (mem::size_of::<usize>() * 8) as u32;

#[cfg(unix)]
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
    }
    .into()
});

#[cfg(windows)]
static MEMORY: LazyLock<Mutex<Memory>> = LazyLock::new(|| {
    use windows_sys::Win32::System::Memory::{
        MEM_COMMIT, MEM_RESERVE, PAGE_EXECUTE_READWRITE, VirtualAlloc,
    };

    let alloc_ptr = unsafe {
        VirtualAlloc(
            ptr::null(),
            DYNAMIC_INST_MEM_SIZE,
            MEM_COMMIT | MEM_RESERVE,
            PAGE_EXECUTE_READWRITE,
        )
    };
    if alloc_ptr.is_null() {
        panic!(
            "failed to initialise memory: {}",
            io::Error::last_os_error()
        );
    }

    let alloc_ptr = alloc_ptr as usize;

    Memory {
        dynamic_inst_ptr_start: alloc_ptr,
        dynamic_inst_ptr: alloc_ptr,
        dynamic_inst_mem_size: DYNAMIC_INST_MEM_SIZE,
    }
    .into()
});

impl Memory {
    fn check_dynamic_inst_ptr(&self) {
        let used = self.dynamic_inst_ptr - self.dynamic_inst_ptr_start;
        if used > self.dynamic_inst_mem_size {
            panic!(
                "ran out of space for dynamically generated x86 instructions, used 0x{used:x} bytes"
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

    use iced_x86::{
        Code, Decoder, DecoderOptions, Encoder, FlowControl, Instruction, MemoryOperand, Register,
        code_asm::*,
    };
    use log::debug;

    let mut decoder = {
        let memory =
            unsafe { slice::from_raw_parts(hook_target as *const u8, isize::MAX as usize) };
        Decoder::new(BITNESS, memory, DecoderOptions::NO_INVALID_CHECK)
    };
    decoder.set_ip(hook_target as u64);
    let target_inst = decoder.decode();
    let target_inst_branch = matches!(
        target_inst.flow_control(),
        FlowControl::UnconditionalBranch
            | FlowControl::IndirectBranch
            | FlowControl::ConditionalBranch
            | FlowControl::Return
            // TODO: not sure how to handle those, so for now im taking it safe
            | FlowControl::Interrupt
            | FlowControl::XbeginXabortXend
            | FlowControl::Exception
    );
    if target_inst.len() < 5 {
        panic!("target instruction is less than 5 bytes");
    }

    // rewrite original jump to custom location
    let memory = &mut MEMORY.lock().unwrap();

    let jmp_diff = memory.dynamic_inst_ptr.abs_diff(hook_target);
    if jmp_diff > i32::MAX as usize {
        debug!("jumping to custom location is more than 32 bits in distance");

        let mut jmp_inst = if cfg!(target_pointer_width = "64") {
            Instruction::with1(
                Code::Jmp_rm64,
                MemoryOperand::with_base_displ(Register::RIP, 0),
            )
            .unwrap()
        } else if cfg!(target_pointer_width = "32") {
            unimplemented!()
        } else {
            unreachable!()
        };
        let jmp_inst_len = {
            let mut encoder = Encoder::new(BITNESS);
            encoder.encode(&jmp_inst, 0).unwrap()
        };

        // find a suitable location for jump addr
        let mut inst = Instruction::new();
        // find location for jumping instruction
        let mut jmp_inst_addr;
        'outer: loop {
            if !decoder.can_decode() {
                panic!("decoder can't read");
            }
            decoder.decode_out(&mut inst);
            jmp_inst_addr = decoder.ip();
            let mut cnt = jmp_inst_len;
            while cnt > 0 {
                if !inst.is_invalid() {
                    continue 'outer;
                }
                cnt -= inst.len();
                if !decoder.can_decode() {
                    panic!("decoder can't read");
                }
                decoder.decode_out(&mut inst);
            }
            break;
        }
        let mut jmp_addr;
        'outer: loop {
            jmp_addr = decoder.ip();
            let mut cnt = mem::size_of::<usize>();
            while cnt > 0 {
                if !inst.is_invalid() {
                    if !decoder.can_decode() {
                        panic!("decoder can't read");
                    }
                    decoder.decode_out(&mut inst);
                    continue 'outer;
                }
                cnt -= inst.len();
                if !decoder.can_decode() {
                    panic!("decoder can't read");
                }
                decoder.decode_out(&mut inst);
            }
            break;
        }
        debug!("jmp destination stored in 0x{jmp_addr:x}, instruction in 0x{jmp_inst_addr:x}");
        jmp_inst.set_memory_displacement64(jmp_addr);

        // write target
        let target = memory.dynamic_inst_ptr.to_le_bytes();
        let orig = make_exec_mem_writable(jmp_addr as usize, target.len())?;
        unsafe { ptr::copy_nonoverlapping(target.as_ptr(), jmp_addr as *mut u8, target.len()) };
        restore_exec_mem_prot(jmp_addr as usize, target.len(), orig)?;

        // write instruction
        let mut encoder = Encoder::new(BITNESS);
        encoder.encode(&jmp_inst, jmp_inst_addr).unwrap();
        let inst = encoder.take_buffer();
        let orig = make_exec_mem_writable(jmp_inst_addr as usize, inst.len())?;
        unsafe { ptr::copy_nonoverlapping(inst.as_ptr(), jmp_inst_addr as *mut u8, inst.len()) };
        restore_exec_mem_prot(jmp_inst_addr as usize, inst.len(), orig)?;

        // actual jmp to the 2nd jmp
        let inst = if cfg!(target_pointer_width = "64") {
            Code::Jmp_rel32_64
        } else if cfg!(target_pointer_width = "32") {
            Code::Jmp_rel32_32
        } else {
            unreachable!()
        };
        let inst = Instruction::with_branch(inst, jmp_inst_addr).unwrap();
        let mut encoder = Encoder::new(BITNESS);
        assert_eq!(encoder.encode(&inst, hook_target as u64).unwrap(), 5);

        let inst = encoder.take_buffer();
        let orig = make_exec_mem_writable(hook_target, inst.len())?;
        unsafe { ptr::copy_nonoverlapping(inst.as_ptr(), hook_target as *mut u8, inst.len()) };
        restore_exec_mem_prot(hook_target, inst.len(), orig)?;
    } else {
        let inst = if cfg!(target_pointer_width = "64") {
            Code::Jmp_rel32_64
        } else if cfg!(target_pointer_width = "32") {
            Code::Jmp_rel32_32
        } else {
            unreachable!()
        };
        let inst = Instruction::with_branch(inst, memory.dynamic_inst_ptr as u64).unwrap();
        let mut encoder = Encoder::new(BITNESS);
        assert_eq!(encoder.encode(&inst, hook_target as u64).unwrap(), 5);

        let inst = encoder.take_buffer();
        let orig = make_exec_mem_writable(hook_target, inst.len())?;
        unsafe { ptr::copy_nonoverlapping(inst.as_ptr(), hook_target as *mut u8, inst.len()) };
        restore_exec_mem_prot(hook_target, inst.len(), orig)?;
    }

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

    // assembler.pushfq().unwrap();
    assembler.push(rax).unwrap();
    /*
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
    */

    assembler.push(rbp).unwrap(); // align sp
    assembler.mov(rbp, rsp).unwrap();
    assembler.and(rsp, -16i32).unwrap();

    assembler.mov(rax, hook as usize as u64).unwrap();
    assembler.call(rax).unwrap();

    assembler.mov(rsp, rbp).unwrap();
    assembler.pop(rbp).unwrap();

    /*
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
    */
    assembler.pop(rax).unwrap();
    // assembler.popfq().unwrap();

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

    #[cfg(windows)]
    flush_instruction_cache();

    memory.dynamic_inst_ptr += instrs.len();
    memory.check_dynamic_inst_ptr();

    Ok(())
}

#[cfg(windows)]
fn flush_instruction_cache() {
    use windows_sys::Win32::System::{
        Diagnostics::Debug::FlushInstructionCache, Threading::GetCurrentProcess,
    };

    if unsafe { FlushInstructionCache(GetCurrentProcess(), ptr::null_mut(), 0) } == 0 {
        panic!(
            "failed to flush instruction cache: {}",
            io::Error::last_os_error()
        );
    }
}

#[cfg(unix)]
fn make_exec_mem_writable(addr: usize, len: usize) -> io::Result<u32> {
    use libc::*;

    let page_size = unsafe { sysconf(_SC_PAGESIZE) } as usize;
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

    Ok((PROT_READ | PROT_EXEC) as u32)
}

#[cfg(windows)]
fn make_exec_mem_writable(addr: usize, len: usize) -> io::Result<u32> {
    use std::ffi::c_void;
    use windows_sys::Win32::System::Memory::{PAGE_EXECUTE_READWRITE, VirtualProtect};

    let old_prot = 0u32;
    if unsafe {
        VirtualProtect(
            addr as *const c_void,
            len,
            PAGE_EXECUTE_READWRITE,
            old_prot as *mut u32,
        )
    } != 0
    {
        return Err(io::Error::last_os_error());
    }

    Ok(old_prot)
}

#[cfg(unix)]
fn restore_exec_mem_prot(addr: usize, len: usize, original: u32) -> io::Result<()> {
    use libc::*;

    let page_size = unsafe { sysconf(_SC_PAGESIZE) } as usize;
    let page_start = addr & !(page_size - 1);
    let aligned_len = ((addr + len + page_size - 1) & !(page_size - 1)) - page_start;
    if unsafe {
        mprotect(
            page_start as *mut libc::c_void,
            aligned_len,
            original as i32,
        )
    } != 0
    {
        return Err(io::Error::last_os_error());
    }

    Ok(())
}

#[cfg(windows)]
fn restore_exec_mem_prot(addr: usize, len: usize, original: u32) -> io::Result<u32> {
    use std::ffi::c_void;
    use windows_sys::Win32::System::Memory::VirtualProtect;

    let old_prot = 0u32;
    if unsafe { VirtualProtect(addr as *const c_void, len, original, old_prot as *mut u32) } != 0 {
        return Err(io::Error::last_os_error());
    }

    Ok(old_prot)
}

#[cfg(unix)]
pub struct MemoryMap(Vec<MemoryMapEntry>);

#[cfg(windows)]
pub struct MemoryMap;

#[cfg(unix)]
struct MemoryMapEntry {
    pub path: std::path::PathBuf,
    pub range: Range<usize>,
}

impl MemoryMap {
    #[cfg(unix)]
    pub fn read_proc_maps() -> io::Result<MemoryMap> {
        use std::{fs::File, io::Read, path::Path};

        let mut maps: Vec<MemoryMapEntry> = Vec::new();
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
                mem.range.end = end_addr;
                continue;
            }

            maps.push(MemoryMapEntry {
                path: file_name.to_path_buf(),
                range: start_addr..end_addr + 1,
            });
        }

        Ok(MemoryMap(maps))
    }

    #[cfg(windows)]
    pub fn read_proc_maps() -> io::Result<MemoryMap> {
        Ok(MemoryMap)
    }

    #[cfg(unix)]
    pub fn find_by_filename(&self, filename: &CStr) -> Option<Range<usize>> {
        let filename = filename.to_string_lossy();
        self.0.iter().find_map(|m_info| {
            if m_info.path.file_name().unwrap().to_string_lossy() == filename {
                Some(m_info.range.clone())
            } else {
                None
            }
        })
    }

    #[cfg(windows)]
    pub fn find_by_filename(&self, filename: &CStr) -> Option<Range<usize>> {
        use windows_sys::Win32::System::{
            LibraryLoader::GetModuleHandleA,
            ProcessStatus::{GetModuleInformation, MODULEINFO},
            Threading::GetCurrentProcess,
        };

        let handle = unsafe { GetModuleHandleA(filename.as_ptr() as *const u8) };
        if handle.is_null() {
            panic!("failed to find exe module, {}", io::Error::last_os_error());
        }

        let mut mod_info = MODULEINFO {
            lpBaseOfDll: ptr::null_mut(),
            SizeOfImage: 0,
            EntryPoint: ptr::null_mut(),
        };
        let res = unsafe {
            GetModuleInformation(
                GetCurrentProcess(),
                handle,
                &mut mod_info as *mut MODULEINFO,
                size_of::<MODULEINFO>() as u32,
            )
        };
        if res == 0 {
            panic!("failed to find exe module, {}", io::Error::last_os_error());
        }
        Some(
            mod_info.lpBaseOfDll as usize
                ..mod_info.lpBaseOfDll as usize + mod_info.SizeOfImage as usize,
        )
    }

    #[cfg(unix)]
    pub fn find_exe(&self) -> Range<usize> {
        use std::env;

        let exe_path = env::current_exe().expect("failed to get current executable path");
        let exe_path = exe_path.to_string_lossy();

        self.0
            .iter()
            .find(|m_info| m_info.path.file_name().unwrap().to_string_lossy() == exe_path)
            .unwrap()
            .range
            .clone()
    }

    #[cfg(windows)]
    pub fn find_exe(&self) -> Range<usize> {
        use windows_sys::Win32::System::{
            LibraryLoader::GetModuleHandleA,
            ProcessStatus::{GetModuleInformation, MODULEINFO},
            Threading::GetCurrentProcess,
        };

        let handle = unsafe { GetModuleHandleA(ptr::null()) };
        if handle.is_null() {
            panic!("failed to find exe module, {}", io::Error::last_os_error());
        }

        let mut mod_info = MODULEINFO {
            lpBaseOfDll: ptr::null_mut(),
            SizeOfImage: 0,
            EntryPoint: ptr::null_mut(),
        };
        let res = unsafe {
            GetModuleInformation(
                GetCurrentProcess(),
                handle,
                &mut mod_info as *mut MODULEINFO,
                size_of::<MODULEINFO>() as u32,
            )
        };
        if res == 0 {
            panic!("failed to find exe module, {}", io::Error::last_os_error());
        }
        mod_info.lpBaseOfDll as usize..mod_info.lpBaseOfDll as usize + mod_info.SizeOfImage as usize
    }
}

pub struct SymbolLookup {
    #[cfg(unix)]
    handles: HashMap<CString, (*mut libc::c_void, HashMap<CString, usize>)>,
    // value contains the handle, and HashMap for symbol and its offset
    #[cfg(windows)]
    handles: HashMap<
        CString,
        (
            windows_sys::Win32::Foundation::HMODULE,
            HashMap<CString, usize>,
        ),
    >,
}

impl SymbolLookup {
    pub fn new() -> Self {
        Self {
            handles: HashMap::new(),
        }
    }

    #[cfg(unix)]
    // TODO: stop trying to redo failed operations
    pub fn get_symbol_in_file(&mut self, file_name: &CStr, symbol: &CStr) -> Option<usize> {
        use libc::*;
        use log::debug;

        let (handle, symbol_map) = match self.handles.get_mut(file_name) {
            Some(handle) => handle,
            None => {
                let handle = unsafe { dlopen(file_name.as_ptr(), RTLD_NOW | RTLD_NOLOAD) };
                if handle.is_null() {
                    debug!("dlopen failure with error: {}", io::Error::last_os_error());
                    return None;
                }
                self.handles
                    .insert(file_name.to_owned(), (handle, HashMap::new()));
                self.handles.get_mut(file_name).unwrap()
            }
        };

        match symbol_map.get(symbol) {
            Some(addr) => Some(*addr),
            None => {
                let symbol_ptr = unsafe { dlsym(*handle, symbol.as_ptr()) };
                if symbol_ptr.is_null() {
                    debug!("dlsym failure with error: {}", io::Error::last_os_error());
                    return None;
                }
                let symbol_ptr = symbol_ptr as usize;
                symbol_map.insert(symbol.to_owned(), symbol_ptr);
                Some(symbol_ptr)
            }
        }
    }

    #[cfg(windows)]
    // TODO: stop trying to redo failed operations
    pub fn get_symbol_in_file(&mut self, file_name: &CStr, symbol: &CStr) -> Option<usize> {
        use log::debug;
        use windows_sys::Win32::System::LibraryLoader::{GetModuleHandleA, GetProcAddress};

        let (handle, symbol_map) = match self.handles.get_mut(file_name) {
            Some(handle) => handle,
            None => {
                let handle = unsafe { GetModuleHandleA(file_name.as_ptr() as *const u8) };
                if handle.is_null() {
                    debug!(
                        "failed to open library with GetModuleHandleA, {}",
                        io::Error::last_os_error()
                    );
                    return None;
                }
                self.handles
                    .insert(file_name.to_owned(), (handle, HashMap::new()));
                self.handles.get_mut(file_name).unwrap()
            }
        };

        match symbol_map.get(symbol) {
            Some(addr) => Some(*addr),
            None => {
                let symbol_ptr = unsafe { GetProcAddress(*handle, symbol.as_ptr() as *const u8) };
                let Some(symbol_ptr) = symbol_ptr else {
                    debug!(
                        "failed to get symbol with GetProcAddress, {}",
                        io::Error::last_os_error()
                    );
                    return None;
                };
                let symbol_ptr = symbol_ptr as usize;
                symbol_map.insert(symbol.to_owned(), symbol_ptr);
                Some(symbol_ptr)
            }
        }
    }
}

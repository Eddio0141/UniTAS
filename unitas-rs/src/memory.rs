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

/// Hooks function at point in memory, targetting jmp instructions that is 5 bytes in length
///
/// # Args
/// - `hook_target`: address where the `hook` should be installed in
/// - `hook`: function to be called
///
/// # Panics
/// - `hook_target` is not pointing to `jmp rel32` (0xE9)
///
/// # Safety
/// - There are no checks if `hook_target` is a valid memory address
/// - The only memory check done here is if the `hook_target` is pointing to `jmp rel32` (0xE9)
#[cfg(all(unix, any(target_arch = "x86_64", target_arch = "x86")))]
pub unsafe fn hook_jmp_32(mut hook_target: usize, hook: extern "C" fn()) -> io::Result<()> {
    let hook = hook as usize;

    if unsafe { *(hook_target as *const u8) } != 0xe9 {
        panic!("hook target is not targetting `jmp`");
    }
    hook_target += 1;

    // save before overwriting
    let jmp_dst = (hook_target + 4) as isize + unsafe { *(hook_target as *const i32) } as isize;

    // rewrite original jump to custom location
    let memory = &mut MEMORY.lock().unwrap();

    unsafe { make_exec_mem_writable(hook_target, 4, memory.page_size) }?;
    let offset = memory.dynamic_inst_ptr as isize - (hook_target + 4) as isize;
    unsafe { *(hook_target as *mut i32) = offset as i32 };
    unsafe { restore_exec_mem_prot(hook_target, 4, memory.page_size) }?;

    // call hook from custom location
    unsafe { *(memory.dynamic_inst_ptr as *mut u8) = 0x50 }; // push eax / rax
    memory.dynamic_inst_ptr += 1;

    if cfg!(target_pointer_width = "64") {
        let inst = [0x48, 0xb8];
        unsafe {
            ptr::copy_nonoverlapping(
                inst.as_ptr(),
                memory.dynamic_inst_ptr as *mut u8,
                inst.len(),
            )
        }; // mov rax
        memory.dynamic_inst_ptr += inst.len();
    } else if cfg!(target_pointer_width = "32") {
        unsafe { *(memory.dynamic_inst_ptr as *mut u8) = 0xb8 }; // mov eax
        memory.dynamic_inst_ptr += 1;
    } else {
        unreachable!();
    }
    unsafe { *(memory.dynamic_inst_ptr as *mut usize) = hook };
    memory.dynamic_inst_ptr += mem::size_of::<usize>();

    let inst = [0xff, 0xd0];
    unsafe {
        ptr::copy_nonoverlapping(
            inst.as_ptr(),
            memory.dynamic_inst_ptr as *mut u8,
            inst.len(),
        )
    }; // call eax / rax
    memory.dynamic_inst_ptr += inst.len();

    unsafe { *(memory.dynamic_inst_ptr as *mut u8) = 0x58 }; // pop eax / rax
    memory.dynamic_inst_ptr += 1;

    unsafe { *(memory.dynamic_inst_ptr as *mut u8) = 0xe9 }; // jmp
    let offset = jmp_dst as i32 - (memory.dynamic_inst_ptr as i32 + 5);
    memory.dynamic_inst_ptr += 1;
    unsafe { *(memory.dynamic_inst_ptr as *mut i32) = offset };
    memory.dynamic_inst_ptr += 4;

    memory.check_dynamic_inst_ptr();

    Ok(())
}

#[cfg(all(unix, any(target_arch = "x86_64", target_arch = "x86")))]
unsafe fn make_exec_mem_writable(addr: usize, len: usize, page_size: usize) -> io::Result<()> {
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
unsafe fn restore_exec_mem_prot(addr: usize, len: usize, page_size: usize) -> io::Result<()> {
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

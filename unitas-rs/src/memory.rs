use std::{
    collections::HashMap,
    ffi::{CStr, CString},
    io,
    ops::Range,
};

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

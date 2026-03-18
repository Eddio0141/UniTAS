use core::slice;
use std::{
    collections::HashMap,
    env,
    ffi::{CStr, CString},
    io,
    ops::Range,
    sync::{Arc, LazyLock, Mutex},
};

pub const PLAYER_MAIN_SYMBOL: &CStr = c"_Z10PlayerMainiPPc";

pub const UNITY_PLAYER_MODULE: &CStr =
    const_str::cstr!(const_str::format!("UnityPlayer{}", env::consts::DLL_SUFFIX));

use iced_x86::{Decoder, DecoderOptions, Instruction};
use pattern::Pattern;

pub static SEARCH: LazyLock<Search> = LazyLock::new(Search::new);

#[derive(Debug)]
pub struct Search {
    modules: MemoryMap,
    symbol_lookup: Arc<Mutex<SymbolLookup>>,
}

impl Search {
    pub fn new() -> Self {
        let modules = MemoryMap::read_proc_maps().expect("couldn't read memory map");
        Self {
            modules,
            symbol_lookup: Arc::new(Mutex::new(SymbolLookup::new())),
        }
    }

    pub fn search<'a>(&'a self) -> SearchStep<'a> {
        SearchStep {
            search: self,
            window: 0..(isize::MAX as usize),
            memory: &[],
            module: None,
        }
    }
}

#[derive(Debug, Clone)]
pub struct SearchStep<'a> {
    search: &'a Search,
    window: Range<usize>,
    memory: &'a [u8],
    /// Last looked up module
    module: Option<&'a CStr>,
}

impl<'a> SearchStep<'a> {
    fn update_memory(&self) -> Self {
        let mut ret = self.clone();
        let mem_start = ret.window.start;
        let mem_end = ret.window.end;
        ret.memory = unsafe { slice::from_raw_parts(mem_start as *const u8, mem_end - mem_start) };
        ret
    }

    pub fn set_start(&self, value: usize) -> Self {
        let mut ret = self.clone();
        ret.window.start = value;
        ret
    }

    pub fn set_end(&self, value: usize) -> Self {
        let mut ret = self.clone();
        ret.window.end = value;
        ret
    }

    pub fn start(&self) -> usize {
        self.window.start
    }

    pub fn memory(&self) -> &'a [u8] {
        self.update_memory().memory
    }

    pub fn module(&mut self, file_name: &'a CStr) -> Option<Self> {
        let window = self.search.modules.find_by_filename(file_name)?;
        let mut ret = self.clone();
        ret.window = window;
        ret.module = Some(file_name);
        Some(ret)
    }

    pub fn exe_module(&self) -> Self {
        let window = self.search.modules.find_exe();
        let mut ret = self.clone();
        ret.window = window;
        // TODO: what to do with self.module?
        ret
    }

    pub fn symbol(&self, symbol: &CStr) -> Option<Self> {
        let file_name = self.module?;
        let offset = self
            .search
            .symbol_lookup
            .lock()
            .unwrap()
            .get_symbol_in_file(file_name, symbol)?;
        let mut ret = self.clone();
        ret.window.start = offset;
        Some(ret)
    }

    pub fn pattern(&self, pattern: Pattern) -> Option<Self> {
        let mut ret = self.update_memory();
        let offset = pattern.matches(ret.memory)? + ret.window.start;
        ret.window.start = offset;
        Some(ret)
    }

    pub fn find_instruction<F: FnMut(Instruction) -> bool>(&self, mut search: F) -> Option<Self> {
        let mut ret = self.update_memory();

        let mut decoder = Decoder::with_ip(
            usize::BITS,
            ret.memory,
            ret.window.start as u64,
            DecoderOptions::NONE,
        );

        let mut instruction = Instruction::default();

        while decoder.can_decode() {
            decoder.decode_out(&mut instruction);

            if search(instruction) {
                ret.window.start = instruction.ip() as usize;
                return Some(ret);
            }
        }

        None
    }
}

#[cfg(unix)]
#[derive(Debug)]
struct MemoryMap(Vec<MemoryMapEntry>);

#[cfg(windows)]
#[derive(Debug)]
struct MemoryMap;

#[cfg(unix)]
#[derive(Debug)]
struct MemoryMapEntry {
    path: std::path::PathBuf,
    range: Range<usize>,
}

impl MemoryMap {
    #[cfg(unix)]
    fn read_proc_maps() -> io::Result<MemoryMap> {
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
    fn read_proc_maps() -> io::Result<MemoryMap> {
        Ok(MemoryMap)
    }

    #[cfg(unix)]
    fn find_by_filename(&self, filename: &CStr) -> Option<Range<usize>> {
        let filename = filename.to_string_lossy();
        self.0.iter().find_map(|m_info| {
            if m_info
                .path
                .file_name()
                .unwrap()
                .to_string_lossy()
                .starts_with(&*filename)
            {
                Some(m_info.range.clone())
            } else {
                None
            }
        })
    }

    #[cfg(windows)]
    fn find_by_filename(&self, filename: &CStr) -> Option<Range<usize>> {
        use windows_sys::Win32::System::{
            LibraryLoader::GetModuleHandleA,
            ProcessStatus::{GetModuleInformation, MODULEINFO},
            Threading::GetCurrentProcess,
        };

        use std::ptr;

        let handle = unsafe { GetModuleHandleA(filename.as_ptr() as *const u8) };
        if handle.is_null() {
            return None;
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
            return None;
        }
        Some(
            mod_info.lpBaseOfDll as usize
                ..mod_info.lpBaseOfDll as usize + mod_info.SizeOfImage as usize,
        )
    }

    #[cfg(unix)]
    fn find_exe(&self) -> Range<usize> {
        use std::env;

        let exe_path = env::current_exe().expect("failed to get current executable path");
        let exe_path = exe_path.to_string_lossy();

        // TODO: this fails on unity 2022.2 x86_64-linux
        self.0
            .iter()
            .find(|m_info| m_info.path.file_name().unwrap().to_string_lossy() == exe_path)
            .unwrap()
            .range
            .clone()
    }

    #[cfg(windows)]
    fn find_exe(&self) -> Range<usize> {
        use std::ptr;

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

#[derive(Debug)]
struct SymbolLookup {
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

unsafe impl Send for SymbolLookup {}

impl SymbolLookup {
    fn new() -> Self {
        Self {
            handles: HashMap::new(),
        }
    }

    #[cfg(unix)]
    // TODO: stop trying to redo failed operations
    fn get_symbol_in_file(&mut self, file_name: &CStr, symbol: &CStr) -> Option<usize> {
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
    fn get_symbol_in_file(&mut self, file_name: &CStr, symbol: &CStr) -> Option<usize> {
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

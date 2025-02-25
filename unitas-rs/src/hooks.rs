use std::{
    env,
    ffi::{CStr, CString},
    slice,
};

use log::{debug, info};
use pattern::Pattern;
use pattern_macro::pattern;

use crate::memory::{self, MemoryMap, SymbolLookup};

trait Hook {
    fn searches(&self) -> &'static [(Search, HookInstall)];
}

struct Search {
    pattern: Pattern,
    start_symbol: Option<&'static CStr>,
    /// Module to search for
    /// - `None` will make it search in the executable itself
    module: Option<&'static CStr>,
}

#[cfg(unix)]
const UNITY_PLAYER_MODULE: &CStr = c"UnityPlayer.so";

pub fn install() {
    let hooks: &[&dyn Hook] = &[&LastUpdate];

    let modules = MemoryMap::read_proc_maps().expect("couldn't read memory map");
    let mut symbol_lookup = SymbolLookup::new();
    info!("installing {} hooks", hooks.len());
    'hooks: for hook in hooks {
        let hooks = hook.searches();
        debug!("hook: attempting {} hook searches", hooks.len());
        for (search, hook_install) in hooks {
            debug!(
                "hook: targeting module {}",
                search
                    .module
                    .map(|m| m.to_string_lossy())
                    .unwrap_or("<executable>".into())
            );
            let mut get_mem_range = |base: &MemoryMap, file| match search.start_symbol {
                Some(start_symbol) => {
                    debug!("hook: targeting symbol {}", start_symbol.to_string_lossy());
                    let mem_offset = search
                        .start_symbol
                        .map(|s| symbol_lookup.get_symbol_in_file(file, s) - base.start)
                        .unwrap_or_default();
                    (base.start + mem_offset, base.end)
                }
                None => (base.start, base.end),
            };
            let (base, mem_start, mem_end) = match search.module {
                Some(m) => {
                    let base = modules
                        .iter()
                        .find(|m_info| {
                            m_info
                                .path
                                .file_name()
                                .expect("module path somehow doesn't have a file name")
                                .to_string_lossy()
                                == m.to_string_lossy()
                        })
                        .expect("failed to find module");
                    let (mem_start, mem_end) = get_mem_range(base, m);
                    (base, mem_start, mem_end)
                }
                None => {
                    let exe_path =
                        env::current_exe().expect("failed to get current executable path");
                    let base = modules
                        .iter()
                        .find(|m_info| m_info.path == exe_path)
                        .expect("failed to find executable address");
                    let exe_path_cstr = CString::new(exe_path.to_string_lossy().as_ref()).unwrap();
                    let (mem_start, mem_end) = get_mem_range(base, &exe_path_cstr);
                    (base, mem_start, mem_end)
                }
            };
            let memory =
                unsafe { slice::from_raw_parts(mem_start as *const u8, mem_end - mem_start) };
            debug!("hook: search addr range: 0x{mem_start:x}..0x{mem_end:x}");

            let Some(offset) = search.pattern.matches(memory) else {
                debug!("search: failed to match pattern!");
                continue;
            };
            let offset = offset + mem_start;

            debug!(
                "search: found pattern in 0x{offset:x} (0x{:x})",
                offset - base.start
            );
            hook_install(offset);

            continue 'hooks;
        }
        panic!("failed to install hook, no patterns matches");
    }
}

type HookInstall = fn(addr: usize);

struct LastUpdate;

impl Hook for LastUpdate {
    fn searches(&self) -> &'static [(Search, HookInstall)] {
        fn install_jmp_32(addr: usize) {
            unsafe { memory::hook_jmp_32(addr, LastUpdate::hook) }
                .expect("failed to install jmp hook");
        }

        const {
            &[(
                // 2022.2.0f1 - 2022.3.41f1 x64 linux
                Search {
                    pattern: pattern!(
                        10,
                        "e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e9 ?? ?? ?? ?? e8 ?? ?? ?? ?? 8b",
                    ),
                    start_symbol: Some(c"_Z10PlayerMainiPPc"),
                    module: Some(UNITY_PLAYER_MODULE),
                },
                install_jmp_32,
            )]
        }
    }
}

impl LastUpdate {
    extern "C" fn hook() {
        println!("hi!");
    }
}

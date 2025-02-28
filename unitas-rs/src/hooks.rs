use std::{
    env,
    ffi::{CStr, CString},
    ops::Range,
    slice,
};

use log::{debug, info};
use pattern::Pattern;
use pattern_macro::pattern;

use crate::memory::{self, MemoryMap, SymbolLookup};

trait Hook {
    /// A list of searches
    fn searches<'a>(&self) -> &'a [(Search, &dyn Fn(usize))];
}

struct Search {
    pattern: Pattern,
    start_symbol: Option<&'static CStr>,
    /// Module to search for
    /// - `None` will make it search in the executable itself
    module: Option<&'static CStr>,
}

const UNITY_PLAYER_MODULE: &CStr =
    const_str::cstr!(const_str::format!("UnityPlayer{}", env::consts::DLL_SUFFIX));

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
            let mut get_mem_range = |base: &Range<usize>, file| match search.start_symbol {
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
                    let base = modules.find_by_filename(m).expect("failed to find module");
                    let (mem_start, mem_end) = get_mem_range(&base, m);
                    (base, mem_start, mem_end)
                }
                None => {
                    let exe_path =
                        env::current_exe().expect("failed to get current executable path");
                    let base = modules.find_exe();
                    let exe_path_cstr = CString::new(exe_path.to_string_lossy().as_ref()).unwrap();
                    let (mem_start, mem_end) = get_mem_range(&base, &exe_path_cstr);
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

struct LastUpdate;

impl Hook for LastUpdate {
    fn searches<'a>(&self) -> &'a [(Search, &dyn Fn(usize))] {
        const {
            &[
                // x86_64-linux
                // 2019.4.40f1 - 2023.2.20f1
                (
                    Search {
                        pattern: pattern!(
                            22,
                            "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8",
                        ),
                        start_symbol: Some(c"_Z10PlayerMainiPPc"),
                        module: Some(UNITY_PLAYER_MODULE),
                    },
                    &|addr| unsafe { memory::hook_inject(addr, false, LastUpdate::hook) }.unwrap(),
                ),
                // x86_64-linux
                // 2017.4.6f1 - 2018.1.5f1
                (
                    Search {
                        pattern: pattern!(
                            12,
                            "0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? eb"
                        ),
                        start_symbol: None,
                        module: None,
                    },
                    &|addr| unsafe { memory::hook_inject(addr, true, LastUpdate::hook) }.unwrap(),
                ),
            ]
        }
    }
}

pub type LastUpdateCallbackFn = unsafe extern "C" fn();
pub static mut LAST_UPDATE_CALLBACK: Option<LastUpdateCallbackFn> = None;

impl LastUpdate {
    extern "C" fn hook() {
        if let Some(callback) = unsafe { LAST_UPDATE_CALLBACK } {
            unsafe { callback() };
        }
    }
}

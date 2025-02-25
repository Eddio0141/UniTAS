use std::{ffi::CStr, slice};

use log::{debug, info};
use pattern::Pattern;
use pattern_macro::pattern;

use crate::memory::{self, MemoryMap, SymbolLookup};

trait Hook {
    fn module(&self) -> &'static CStr;
    fn searches(&self) -> &'static [(Search, HookInstall)];
}

struct Search {
    pattern: Pattern,
    start_symbol: Option<&'static CStr>,
}

#[cfg(unix)]
const UNITY_PLAYER_MODULE: &CStr = c"UnityPlayer.so";

pub fn install() {
    let hooks: &[&dyn Hook] = &[&LastUpdate];

    let modules = MemoryMap::read_proc_maps().expect("couldn't read memory map");
    let mut symbol_lookup = SymbolLookup::new();
    info!("installing {} hooks", hooks.len());
    'hooks: for hook in hooks {
        let base = hook.module();
        debug!("hook: targeting module {}", base.to_string_lossy());
        let base = modules
            .get(base)
            .expect("failed to get base address for module");
        debug!(
            "hook: base addr range: 0x{:x}..0x{:x}",
            base.start, base.end
        );
        let memory =
            unsafe { slice::from_raw_parts(base.start as *const u8, base.end - base.start) };

        let hooks = hook.searches();
        debug!("hook: attempting {} hook searches", hooks.len());
        for (offset, hook_install) in hooks {
            debug!("search: start symbol: {:?}", offset.start_symbol);
            let mem_offset = offset
                .start_symbol
                .map(|s| symbol_lookup.get_symbol_in_file(UNITY_PLAYER_MODULE, s) - base.start)
                .unwrap_or_default();
            debug!("search: symbol offset is 0x{mem_offset:x}");
            let Some(offset) = offset.pattern.matches(&memory[mem_offset..]) else {
                debug!("search: failed to match pattern!");
                continue;
            };

            let rel_offset = offset + mem_offset;
            let offset = base.start + rel_offset;
            debug!("search: found pattern in 0x{offset:x} (0x{rel_offset:x})",);
            hook_install(offset);

            continue 'hooks;
        }
        panic!("failed to install hook, no patterns matches");
    }
}

type HookInstall = fn(addr: usize);

struct LastUpdate;

impl Hook for LastUpdate {
    fn module(&self) -> &'static CStr {
        UNITY_PLAYER_MODULE
    }

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

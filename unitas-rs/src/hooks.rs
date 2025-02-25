use std::slice;

use pattern::Pattern;
use pattern_macro::pattern;

use crate::memory::{self, MemoryMap};

trait Hook {
    fn module(&self) -> &'static str;
    fn pattern(&self) -> &'static [(Pattern, HookInstall)];
}

#[cfg(unix)]
const UNITY_PLAYER_MODULE: &str = "UnityPlayer.so";

pub fn install() {
    let hooks: &[&dyn Hook] = &[&LastUpdate];

    let modules = MemoryMap::read_proc_maps().expect("couldn't read memory map");
    for hook in hooks {
        let base = modules
            .get(hook.module())
            .expect("failed to get base address for module");
        let memory =
            unsafe { slice::from_raw_parts(base.start as *const u8, base.end - base.start) };

        let Some((offset, hook_install)) = hook
            .pattern()
            .iter()
            .find_map(|p| p.0.matches(memory).map(|offset| (offset, p.1)))
        else {
            panic!("failed to install hook, no patterns matches");
        };
        hook_install(base.start + offset);
    }
}

type HookInstall = fn(addr: usize);

struct LastUpdate;

impl Hook for LastUpdate {
    fn module(&self) -> &'static str {
        UNITY_PLAYER_MODULE
    }

    fn pattern(&self) -> &'static [(Pattern, HookInstall)] {
        fn install_jmp_32(addr: usize) {
            unsafe { memory::hook_jmp_32(addr, LastUpdate::hook) }
                .expect("failed to install jmp hook");
        }

        const {
            &[(
                // 2022.2.0f1 - 2022.3.41f1 x64 linux
                // TODO: it seems like symbol exists for `PlayerMain`, which this hook attaches to
                //       this may narrow the scope of search and make things cleaner and accurate, so i should probably support this
                pattern!(
                    10,
                    "e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e9 ?? ?? ?? ?? e8 ?? ?? ?? ?? 8b",
                ),
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

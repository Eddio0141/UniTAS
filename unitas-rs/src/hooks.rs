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
                // 2022.2.0f1 x64 linux
                // TODO: make it more generic
                pattern!(
                    52,
                    "e8 cd 7a db ff 83 f8 01 75 19 bf 02 00 00 00 be 01 00 00 00 e8 d9 dc db ff bf 01 00 00 00 e8 7f fd 00 00 0f b6 3d c0 fa 1b 01 e8 13 86 e7 ff e8 3e 8c db ff e9",
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

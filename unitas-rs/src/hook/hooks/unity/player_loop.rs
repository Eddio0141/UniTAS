use retour::RawMidFuncHook;

use super::UNITY_PLAYER_MODULE;
use crate::hook::installer::{Hook, Search};
use pattern_macro::pattern;

pub type LastUpdateCallbackFn = unsafe extern "C" fn();

pub static mut LAST_UPDATE_CALLBACK: Option<LastUpdateCallbackFn> = None;

#[cfg(all(unix, target_arch = "x86_64"))]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    use std::ffi::CStr;

    const PLAYER_MAIN_SYMBOL: &CStr = c"_Z10PlayerMainiPPc";

    Hook(
        const {
            &[
                // 2019.4.40f1 - 6000.0.0b11
                Search::new(&|addr| last_update_mid_func_install(addr, false))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(22, "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8"))
                    .with_symbol(PLAYER_MAIN_SYMBOL),
                // 6000.0.25f1 - 6000.0.40f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(6, "8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? ?? 8d"))
                    .with_symbol(PLAYER_MAIN_SYMBOL),
                // 2017.4.6f1 - 2018.1.5f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(12, "0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? eb")),
            ]
        },
    )
}

#[cfg(all(windows, target_arch = "x86"))]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    Hook(
        const {
            &[
                // 6000.0.41f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(
                        18,
                        "83 ?? ?? 75 ?? 8d ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e8"
                    )),
                // 2022.2.21f1 - 2023.1.0f1
                Search::new(&|addr| last_update_mid_func_install(addr, false))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(
                        0,
                        "83 3d ?? ?? ?? ?? ?? 74 ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e8"
                    )),
                // 2019.3.8f1 - 2021.3.45f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(
                        18,
                        "75 ?? ?? 6a ?? e8 ?? ?? ?? ?? 83 ?? ?? e8 ?? ?? ?? ?? e8"
                    )),
                // 2017.3.0f? - 2018.4.25f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(
                        18,
                        "83 ?? ?? e8 ?? ?? ?? ?? 6a ?? e8 ?? ?? ?? ?? 83 ?? ?? e8"
                    )),
            ]
        },
    )
}

#[cfg(all(windows, target_arch = "x86_64"))]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    Hook(
        const {
            &[
                // 2022.2.0f1 - 6000.0.41f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(5, "e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? ?? 85 ?? 74 ?? ?? 38 ?? ?? ?? ?? ?? 74")),
                // 2019.3.7f1 - 2021.3.10f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(21, "83 ?? ?? 75 ?? 0f b6 ?? 8d ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e8")),
                // 2017.4.22f1 - 2018.4.1f1
                Search::new(&|addr| last_update_mid_func_install(addr, true))
                    .with_module(UNITY_PLAYER_MODULE)
                    .with_pattern(pattern!(20, "8d ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8")),
            ]
        },
    )
}

fn last_update_mid_func_install(addr: usize, original_first: bool) {
    static mut LAST_UPDATE_HOOK: Option<RawMidFuncHook> = None;

    let hook = unsafe {
        RawMidFuncHook::new(
            addr as *const (),
            last_update_hook_callback as *const (),
            original_first,
        )
    }
    .unwrap();
    unsafe { hook.enable() }.unwrap();
    unsafe { LAST_UPDATE_HOOK = Some(hook) };
}

fn last_update_hook_callback() {
    unsafe { LAST_UPDATE_CALLBACK.expect("last update callback is not set")() };
}

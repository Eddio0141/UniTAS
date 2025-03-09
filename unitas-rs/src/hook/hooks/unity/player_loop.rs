use retour::RawMidFuncHook;

use super::UNITY_PLAYER_MODULE;
use crate::hook::installer::{Hook, Search};
use pattern_macro::pattern;

pub type LastUpdateCallbackFn = unsafe extern "C" fn();

pub static mut LAST_UPDATE_CALLBACK: Option<LastUpdateCallbackFn> = None;

#[cfg(unix)]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    use std::ffi::CStr;

    const PLAYER_MAIN_SYMBOL: &CStr = c"_Z10PlayerMainiPPc";

    Hook(
        const {
            &[
                // 2019.4.40f1 - 6000.0.0b11
                Search::new(&|addr| last_update_mid_func_install(addr, false)).with_module(UNITY_PLAYER_MODULE).with_pattern(pattern!(22, "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8")).with_symbol(PLAYER_MAIN_SYMBOL),
                // 6000.0.25f1 - 6000.0.40f1
                Search::new(&|addr| last_update_mid_func_install(addr, true)).with_module(UNITY_PLAYER_MODULE).with_pattern(pattern!(6, "8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? ?? 8d")).with_symbol(PLAYER_MAIN_SYMBOL),
                // 2017.4.6f1 - 2018.1.5f1
                Search::new(&|addr| last_update_mid_func_install(addr, true)).with_module(UNITY_PLAYER_MODULE).with_pattern(pattern!(12, "0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? eb")),
            ]
        },
    )
}

#[cfg(windows)]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    Hook(
        const {
            &[
                // 2017.4.22f1
                Search::new(&|addr| last_update_mid_func_install(addr, true)).with_module(UNITY_PLAYER_MODULE).with_pattern(pattern!(12, "e8 ?? ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 83 ?? ?? e9 ?? ?? ?? ?? e8")),
                // 2022.2.0f1
                Search::new(&|addr| last_update_mid_func_install(addr, true)).with_module(UNITY_PLAYER_MODULE).with_pattern(pattern!(5, "e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? ?? 85 ?? 74 ?? ?? 38 ?? ?? ?? ?? ?? 74")),
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

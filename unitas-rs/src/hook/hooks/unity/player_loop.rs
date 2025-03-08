use retour::RawMidFuncHook;

use super::UNITY_PLAYER_MODULE;
use crate::hook::installer::{Hook, Search};
use pattern_macro::pattern;

pub type LastUpdateCallbackFn = unsafe extern "C" fn();

pub static mut LAST_UPDATE_CALLBACK: Option<LastUpdateCallbackFn> = None;

#[cfg(unix)]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    Hook(
        const {
            &[
                // x86_64-linux
                // 2019.4.40f1 - 6000.0.0b11
                Search {
                    pattern: Some(pattern!(
                        22,
                        "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8"
                    )),
                    start_symbol: Some(c"_Z10PlayerMainiPPc"),
                    module: Some(UNITY_PLAYER_MODULE),
                    installer: &|addr| {
                        last_update_mid_func_install(addr, false);
                    },
                },
                // x86_64-linux
                // 6000.0.25f1 - 6000.0.40f1
                Search {
                    pattern: Some(pattern!(
                        6,
                        "8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? ?? 8d"
                    )),
                    start_symbol: Some(c"_Z10PlayerMainiPPc"),
                    module: Some(UNITY_PLAYER_MODULE),
                    installer: &|addr| last_update_mid_func_install(addr, true),
                },
                // x86_64-linux
                // 2017.4.6f1 - 2018.1.5f1
                Search {
                    pattern: Some(pattern!(
                        12,
                        "0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? eb"
                    )),
                    start_symbol: None,
                    module: None,
                    installer: &|addr| last_update_mid_func_install(addr, true),
                },
            ]
        },
    )
}

#[cfg(windows)]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    Hook(
        const {
            &[
                // win64
                // 2022.2.0f1
                Search {
                    pattern: Some(pattern!(
                        5,
                        "e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? ?? 85 ?? 74 ?? ?? 38 ?? ?? ?? ?? ?? 74"
                    )),
                    start_symbol: None,
                    module: Some(UNITY_PLAYER_MODULE),
                    installer: &|addr| last_update_mid_func_install(addr, true),
                },
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

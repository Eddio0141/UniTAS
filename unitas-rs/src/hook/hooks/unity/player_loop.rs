#[cfg(all(unix, target_arch = "x86_64"))]
use crate::memory::Search;

use crate::memory::{PLAYER_MAIN_SYMBOL, UNITY_PLAYER_MODULE};
use pattern_macro::pattern;
use retour::RawMidFuncHook;

pub type LastUpdateCallbackFn = unsafe extern "C" fn();

pub static mut LAST_UPDATE_CALLBACK: Option<LastUpdateCallbackFn> = None;

#[cfg(all(unix, target_arch = "x86_64"))]
pub fn last_update_hook(search: &Search) {
    let main_symbol = search
        .search()
        .module(UNITY_PLAYER_MODULE)
        .expect("failed to find unity player module")
        .symbol(PLAYER_MAIN_SYMBOL)
        .expect("failed to find unity main symbol");

    // 2019.4.40f1 - 6000.0.0b11
    if let Some(addr) = main_symbol.pattern(pattern!(
        22,
        "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8"
    )) {
        last_update_mid_func_install(addr.start(), false);
    }
    // 6000.0.25f1 - 6000.0.40f1
    if let Some(addr) = main_symbol
        .pattern(pattern!(
            6,
            "8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? ?? 8d"
        ))
        .or_else(|| {
            // 2017.4.6f1 - 2018.1.5f1
            search
                .search()
                .module(UNITY_PLAYER_MODULE)
                .expect("failed to find unity player module")
                .pattern(pattern!(
                    12,
                    "0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? eb"
                ))
        })
    {
        last_update_mid_func_install(addr.start(), true);
    }
}

#[cfg(all(windows, target_arch = "x86"))]
pub fn last_update_hook(search: &Search) {
    let module = search
        .search()
        .module(UNITY_PLAYER_MODULE)
        .expect("failed to find unity main symbol");

    // 6000.0.41f1
    if let Some(addr) = module.pattern(pattern!(
        18,
        "83 ?? ?? 75 ?? 8d ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e8"
    )) {
        last_update_mid_func_install(addr.start(), true);
    }
    // 2022.2.21f1 - 2023.1.0f1
    if let Some(addr) = module.pattern(pattern!(
        0,
        "83 3d ?? ?? ?? ?? ?? 74 ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e8"
    )) {
        last_update_mid_func_install(addr.start(), false);
    }
    // 2019.3.8f1 - 2021.3.45f1
    if let Some(addr) = module
        .pattern(pattern!(
            18,
            "75 ?? ?? 6a ?? e8 ?? ?? ?? ?? 83 ?? ?? e8 ?? ?? ?? ?? e8"
        ))
        .or_else(|| {
            // 2017.3.0f? - 2018.4.25f1
            module.pattern(pattern!(
                18,
                "83 ?? ?? e8 ?? ?? ?? ?? 6a ?? e8 ?? ?? ?? ?? 83 ?? ?? e8"
            ))
        })
        .or_else(|| {
            // 5.4.0f3
            // NOTE: the instruction mnemonics are the same with 4.2.2f1
            search.search().pattern(pattern!(
                17,
                "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? 6a ?? 6a ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? e9"
            ))
        })
        .or_else(|| {
            // 4.2.2f1 - 5.0.2f1
            search.search().pattern(pattern!(
                16,
                "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? ?? 6a ?? ?? e8 ?? ?? ?? ?? 83"
            ))
        })
    {
        last_update_mid_func_install(addr.start(), true);
    }
}

#[cfg(all(windows, target_arch = "x86_64"))]
pub fn last_update_hook(search: &Search) {
    let module = search
        .search()
        .module(UNITY_PLAYER_MODULE)
        .expect("failed to find unity main symbol");

    // 2022.2.0f1 - 6000.0.41f1
    let addr = module.pattern(pattern!(5, "e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? ?? 85 ?? 74 ?? ?? 38 ?? ?? ?? ?? ?? 74"))
    // 2019.3.7f1 - 2021.3.10f1
        .or_else(|| module.pattern(pattern!(21, "83 ?? ?? 75 ?? 0f b6 ?? 8d ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? e8")))
    // 2017.4.22f1 - 2018.4.1f1
        .or_else(|| module.pattern(pattern!(20, "8d ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8")))
    .expect("failed to find pattern");

    last_update_mid_func_install(addr.start(), true);
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

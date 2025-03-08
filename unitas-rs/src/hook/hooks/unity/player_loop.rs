use crate::hook::Hook;

pub type LastUpdateCallbackFn = unsafe extern "C" fn();

pub static mut LAST_UPDATE_CALLBACK: Option<LastUpdateCallbackFn> = None;

#[cfg(unix)]
pub const fn last_update_hook<'a>() -> Hook<'a> {
    use std::mem;

    use pattern_macro::pattern;
    use retour::{RawMidFuncHook, static_detour};

    use crate::hook::installer::{Hook, Search};

    use super::UNITY_PLAYER_MODULE;

    Hook(
        const {
            &[
                // TODO: clean up
                Search {
                    pattern: pattern!(
                        -1,
                        "83 ?? ?? 80 3d ?? ?? ?? ?? ?? 75 ?? ?? 8d ?? ?? ?? ?? ?? 66 ?? 0f 6e"
                    ),
                    start_symbol: None,
                    module: Some(UNITY_PLAYER_MODULE),
                    installer: &|addr| unsafe {
                        static_detour! {
                            static PlayerUpdate: extern "C" fn();
                        }

                        PlayerUpdate
                            .initialize(mem::transmute::<usize, extern "C" fn()>(addr), || {
                                PlayerUpdate.call();
                                last_update_hook_callback();
                            })
                            .unwrap()
                            .enable()
                            .unwrap()
                    },
                },
                // TODO: clean up
                Search {
                    pattern: pattern!(
                        6,
                        "8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? ?? 8d"
                    ),
                    start_symbol: None,
                    module: Some(UNITY_PLAYER_MODULE),
                    installer: &|addr| unsafe {
                        static mut LAST_UPDATE_HOOK: Option<RawMidFuncHook> = None;

                        fn hook() {
                            unsafe {
                                LAST_UPDATE_CALLBACK.expect("last update callback is not set")()
                            };
                        }

                        let hook = RawMidFuncHook::new(addr as *const (), hook as *const (), true)
                            .unwrap();
                        hook.enable().unwrap();
                        LAST_UPDATE_HOOK = Some(hook);
                    },
                },
                // x86_64-linux
                // 2019.4.40f1 - 6000.0.0b11
                // (
                //     Search {
                //         pattern: pattern!(
                //             22,
                //             "e8 ?? ?? ?? ?? 0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8",
                //         ),
                //         start_symbol: Some(c"_Z10PlayerMainiPPc"),
                //         module: Some(UNITY_PLAYER_MODULE),
                //     },
                //     &|addr| unsafe { memory::hook_inject(addr, false, LastUpdate::hook) }.unwrap(),
                // ),
                // // x86_64-linux
                // // 6000.0.25f1 - 6000.0.40f1
                // (
                //     Search {
                //         pattern: pattern!(
                //             6,
                //             "8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? 83 ?? ?? ?? 8d"
                //         ),
                //         start_symbol: Some(c"_Z10PlayerMainiPPc"),
                //         module: Some(UNITY_PLAYER_MODULE),
                //     },
                //     &|addr| unsafe { memory::hook_inject(addr, true, LastUpdate::hook) }.unwrap(),
                // ),
                // // x86_64-linux
                // // 2017.4.6f1 - 2018.1.5f1
                // (
                //     Search {
                //         pattern: pattern!(
                //             12,
                //             "0f b6 ?? ?? ?? ?? ?? e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? eb"
                //         ),
                //         start_symbol: None,
                //         module: None,
                //     },
                //     &|addr| unsafe { memory::hook_inject(addr, true, LastUpdate::hook) }.unwrap(),
                // ),
            ]
        },
    )
}

#[cfg(windows)]
fn last_update_hook<'a>(&self) -> &'a [(Search, &dyn Fn(usize))] {
    const {
        &[
            // win64
            // 2022.2.0f1
            // TODO: clean up
            (
                Search {
                    pattern: pattern!(
                        5,
                        "e8 ?? ?? ?? ?? e8 ?? ?? ?? ?? ?? 8b ?? ?? ?? ?? ?? ?? 85 ?? 74 ?? ?? 38 ?? ?? ?? ?? ?? 74",
                    ),
                    start_symbol: None,
                    module: Some(UNITY_PLAYER_MODULE),
                },
                &|addr| unsafe {
                    static mut HOOK: Option<RawMidFuncHook> = None;

                    fn hook() {
                        unsafe { LAST_UPDATE_CALLBACK.expect("last update callback is not set")() };
                    }

                    let hook =
                        RawMidFuncHook::new(addr as *const (), hook as *const (), true).unwrap();
                    hook.enable().unwrap();
                    HOOK = Some(hook);
                },
            ),
        ]
    }
}

fn last_update_hook_callback() {
    unsafe { LAST_UPDATE_CALLBACK.expect("last update callback is not set")() };
}

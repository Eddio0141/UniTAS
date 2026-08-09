use std::time::Duration;

use libc::*;
use retour::static_detour;

use crate::{detour_setup_log_fail, hook::hooks::REVERSE_INVOKE};

static_detour! {
    static open_detour: unsafe extern "C" fn(*const c_char, c_int) -> c_int;
}

pub fn install_detours() {
    // detour_setup_log_fail!(open_detour, open, |a, b| {
    //     todo!()
    //     // let ret = unsafe { sleep_detour.call(secs) };
    //     //
    //     // if REVERSE_INVOKE.get() {
    //     //     return ret;
    //     // }
    //     //
    //     // // update time based on how many seconds was slept
    //     // TIME.lock()
    //     //     .unwrap()
    //     //     .update(Duration::from_secs((secs - ret) as u64));
    //     // ret
    // });

    let detour = &open_detour;
    let before = open;
    let after = |a, b| todo!();
    if let Err(err) = unsafe {
        detour.initialize(
            std::mem::transmute::<*const (), unsafe extern "C" fn(*const i8, i32) -> i32>(
                before as *const (),
            ),
            after,
        )
    } {
        log::warn!(
            "detour: failed to init `{}`, reason: {err:?}",
            stringify!($detour)
        );
    }
    if let Err(err) = unsafe { detour.enable() } {
        log::warn!(
            "detour: failed to enable `{}`, reason: {err:?}",
            stringify!($detour)
        );
    }
}

extern "C" fn thing(...) {}

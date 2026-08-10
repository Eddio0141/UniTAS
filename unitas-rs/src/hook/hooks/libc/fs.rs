use std::time::Duration;

use libc::*;
use retour::static_detour;

use crate::{detour_setup_log_fail, hook::hooks::REVERSE_INVOKE};

static_detour! {
    static open_detour: unsafe extern "C" fn(*const c_char, c_int) -> c_int;
}

pub fn install_detours() {
    detour_setup_log_fail!(open_detour, open, |secs| {
        // let ret = unsafe { sleep_detour.call(secs) };
        //
        // if REVERSE_INVOKE.get() {
        //     return ret;
        // }
        //
        // // update time based on how many seconds was slept
        // TIME.lock()
        //     .unwrap()
        //     .update(Duration::from_secs((secs - ret) as u64));
        // ret
    });
}

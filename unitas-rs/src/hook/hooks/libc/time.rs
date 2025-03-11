use libc::*;
use retour::static_detour;

use crate::{detour_setup_log_fail, hook::hooks::REVERSE_INVOKE};

static_detour! {
    static clock_gettime_detour: unsafe extern "C" fn(clockid_t, *mut timespec) -> c_int;
}

pub fn install_detours() {
    detour_setup_log_fail!(clock_gettime_detour, clock_gettime, |clk_id, tp| {
        if REVERSE_INVOKE.get() {
            return unsafe { clock_gettime_detour.call(clk_id, tp) };
        }
        // println!("{clk_id}: {tp:?}");
        unsafe { clock_gettime_detour.call(clk_id, tp) }
    });
}

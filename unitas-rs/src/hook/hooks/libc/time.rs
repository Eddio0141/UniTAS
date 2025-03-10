use libc::*;
use retour::static_detour;

use crate::hook::hooks::REVERSE_INVOKE;

static_detour! {
    static clock_gettime_detour: unsafe extern "C" fn(clockid_t, *mut timespec) -> c_int;
}

pub fn install_detours() {
    unsafe {
        clock_gettime_detour
            .initialize(clock_gettime, |clk_id, tp| {
                if REVERSE_INVOKE.get() {
                    return clock_gettime_detour.call(clk_id, tp);
                }
                // println!("{clk_id}: {tp:?}");
                clock_gettime_detour.call(clk_id, tp)
            })
            .unwrap()
            .enable()
    }
    .unwrap();
}

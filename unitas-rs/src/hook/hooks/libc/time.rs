/*
use libc::*;
use log::trace;
use retour::static_detour;

use crate::{detour_setup_log_fail, hook::hooks::REVERSE_INVOKE, info, state::os::time::TIME};

static_detour! {
    static clock_gettime_detour: unsafe extern "C" fn(clockid_t, *mut timespec) -> c_int;
}

pub fn install_detours() {
    detour_setup_log_fail!(
        clock_gettime_detour,
        clock_gettime,
        |clk_id, tp: *mut timespec| {
            if REVERSE_INVOKE.get() || tp.is_null() {
                return unsafe { clock_gettime_detour.call(clk_id, tp) };
            }
            if info::unity::is_main_thread() {
                trace!("main thread");
            }
            match clk_id {
                CLOCK_REALTIME => trace!("clock_gettime: TODO: CLOCK_REALTIME"),
                CLOCK_MONOTONIC | CLOCK_MONOTONIC_RAW | CLOCK_MONOTONIC_COARSE => {
                    let monotonic = TIME.lock().unwrap().monotonic();
                    let tp = unsafe { &mut *tp };
                    tp.tv_sec = monotonic.as_secs() as i64;
                    tp.tv_nsec = monotonic.subsec_nanos() as i64;
                    return 0;
                }
                CLOCK_PROCESS_CPUTIME_ID | CLOCK_THREAD_CPUTIME_ID => {
                    let monotonic_cpu = TIME.lock().unwrap().monotonic_cpu();
                    let tp = unsafe { &mut *tp };
                    tp.tv_sec = monotonic_cpu.as_secs() as i64;
                    tp.tv_nsec = monotonic_cpu.subsec_nanos() as i64;
                    return 0;
                }
                _ => trace!("clock_gettime: unexpected clk_id: {clk_id}"),
            }
            unsafe { clock_gettime_detour.call(clk_id, tp) }
        }
    );
}
*/

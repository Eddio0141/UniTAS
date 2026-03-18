use std::{mem, sync::LazyLock};

use libc::*;
use log::trace;

use crate::{hook::hooks::REVERSE_INVOKE, state::os::TIME, utils};

type ClockGettime = unsafe extern "C" fn(clockid_t, *mut timespec) -> c_int;
type Gettimeofday = unsafe fn(*mut timeval, *mut timezone) -> c_int;
type Clock = unsafe fn(*mut time_t) -> time_t;

struct OrigFuncs {
    clock_gettime: ClockGettime,
    gettimeofday: Gettimeofday,
    clock: Clock,
}

static ORIG_FUNCS: LazyLock<OrigFuncs> = LazyLock::new(|| OrigFuncs {
    clock_gettime: unsafe {
        mem::transmute::<*const (), ClockGettime>(
            utils::unix::orig_sym_addr(c"clock_gettime").expect("failed to find clock_gettime"),
        )
    },
    gettimeofday: unsafe {
        mem::transmute::<*const (), Gettimeofday>(
            utils::unix::orig_sym_addr(c"gettimeofday").expect("failed to find gettimeofday"),
        )
    },
    clock: unsafe {
        mem::transmute::<*const (), Clock>(
            utils::unix::orig_sym_addr(c"clock").expect("failed to find clock"),
        )
    },
});

// #[unsafe(no_mangle)]
// pub unsafe extern "C" fn clock_gettime(clk_id: clockid_t, tp: *mut timespec) -> c_int {
//     if REVERSE_INVOKE.get() || tp.is_null() {
//         return unsafe { (ORIG_FUNCS.clock_gettime)(clk_id, tp) };
//     }
//     if clk_id == CLOCK_REALTIME
//         || clk_id == CLOCK_MONOTONIC
//         || clk_id == CLOCK_MONOTONIC_RAW
//         || clk_id == CLOCK_MONOTONIC_COARSE
//     {
//         let monotonic = TIME.lock().unwrap().get(Some(clk_id));
//         let tp = unsafe { &mut *tp };
//         tp.tv_sec = monotonic.as_secs() as i64;
//         tp.tv_nsec = monotonic.subsec_nanos() as i64;
//         return 0;
//     }
//     trace!("clock_gettime: unexpected clk_id: {clk_id}");
//     unsafe { (ORIG_FUNCS.clock_gettime)(clk_id, tp) }
// }
//
// #[unsafe(no_mangle)]
// pub unsafe fn gettimeofday(tp: *mut timeval, tz: *mut timezone) -> c_int {
//     if REVERSE_INVOKE.get() || tp.is_null() {
//         return unsafe { (ORIG_FUNCS.gettimeofday)(tp, tz) };
//     }
//     // TODO: we ignore tz here, it is noted as deprecated so should be fine? should test on oldest linux unity and decide
//     // clk_id isn't a thing here, just give it the same value
//     let monotonic = TIME.lock().unwrap().get(None);
//     let tp = unsafe { &mut *tp };
//     tp.tv_sec = monotonic.as_secs() as i64;
//     tp.tv_usec = monotonic.subsec_micros() as i64;
//     0
// }
//
// #[unsafe(no_mangle)]
// pub unsafe fn time(time: *mut time_t) -> time_t {
//     if REVERSE_INVOKE.get() {
//         return unsafe { (ORIG_FUNCS.clock)(time) };
//     }
//     // clk_id isn't a thing here, just give it the same value
//     let monotonic = TIME.lock().unwrap().get(None);
//
//     let res = monotonic.as_secs() as i64;
//     if !time.is_null() {
//         // non-null means result also goes in time
//         let time = unsafe { &mut *time };
//         *time = res;
//     }
//     res
// }

// TODO: is it on main thread? because thats the only delay that matters
// TODO: how does it relate to framerate? are sleeps used? find out
// TODO: find out for win32 too

// use std::time::Duration;
//
// use libc::*;
// use retour::static_detour;
//
// use crate::{detour_setup_log_fail, hook::hooks::REVERSE_INVOKE, state::os::time::TIME};
//
// static_detour! {
//     static sleep_detour: unsafe extern "C" fn(c_uint) -> c_uint;
//     static nanosleep_detour: unsafe extern "C" fn (*const timespec, *mut timespec) -> c_int;
// }
//
// pub fn install_detours() {
//     detour_setup_log_fail!(sleep_detour, sleep, |secs| {
//         let ret = unsafe { sleep_detour.call(secs) };
//
//         if REVERSE_INVOKE.get() {
//             return ret;
//         }
//
//         // update time based on how many seconds was slept
//         TIME.lock()
//             .unwrap()
//             .update(Duration::from_secs((secs - ret) as u64));
//         ret
//     });
//     detour_setup_log_fail!(nanosleep_detour, nanosleep, |rqtp, rmtp| { todo!() });
// }

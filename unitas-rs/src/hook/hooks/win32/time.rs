use retour::static_detour;
use windows_sys::Win32::{
    Foundation::{BOOL, TRUE},
    System::Performance::{QueryPerformanceCounter, QueryPerformanceFrequency},
};

use crate::{
    detour_setup_log_fail,
    hook::hooks::REVERSE_INVOKE,
    state::{hardware::CPU_CLOCK_SPEED, os::time::TIME},
};

static_detour! {
    static QueryPerformanceFrequency_detour: unsafe extern "system" fn(*mut i64) -> BOOL;
    static QueryPerformanceCounter_detour: unsafe extern "system" fn(*mut i64) -> BOOL;
}

pub fn install_detours() {
    detour_setup_log_fail!(
        QueryPerformanceFrequency_detour,
        QueryPerformanceFrequency,
        |lpfrequency: *mut i64| {
            if REVERSE_INVOKE.get() || lpfrequency.is_null() {
                return unsafe { QueryPerformanceFrequency_detour.call(lpfrequency) };
            }
            // return as khz
            unsafe { *lpfrequency = (CPU_CLOCK_SPEED / 1000) as i64 };
            TRUE
        }
    );
    detour_setup_log_fail!(
        QueryPerformanceCounter_detour,
        QueryPerformanceCounter,
        |lpperformancecount: *mut i64| {
            if REVERSE_INVOKE.get() || lpperformancecount.is_null() {
                return unsafe { QueryPerformanceCounter_detour.call(lpperformancecount) };
            }
            let lpperformancecount = unsafe { &mut *lpperformancecount };
            let monotonic = TIME.lock().unwrap().monotonic();
            let khz_clock = CPU_CLOCK_SPEED / 1000;
            *lpperformancecount = (monotonic.as_secs() * khz_clock
                + monotonic.subsec_nanos() as u64 * (khz_clock / 1_000_000_000))
                as i64;
            TRUE
        }
    );
}

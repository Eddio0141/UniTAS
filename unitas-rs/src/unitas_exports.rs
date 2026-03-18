use std::time::Duration;

use crate::{hook::hooks, info, state::os::TIME};

#[unsafe(no_mangle)]
pub extern "C" fn last_update_set_callback(
    callback: hooks::unity::player_loop::LastUpdateCallbackFn,
) {
    unsafe { hooks::unity::player_loop::LAST_UPDATE_CALLBACK = Some(callback) };
}

#[unsafe(no_mangle)]
pub extern "C" fn set_frame_time(frame_time: f64) {
    TIME.lock()
        .unwrap()
        .set_frame_time(Duration::from_secs_f64(frame_time));
}

#[unsafe(no_mangle)]
pub extern "C" fn update_actual() {
    info::set_main_thread();
    TIME.lock().unwrap().add_frame_time();
}

#[unsafe(no_mangle)]
pub extern "C" fn restart(secs: u64, nano_secs: u32) {
    TIME.lock().unwrap().restart(Duration::new(secs, nano_secs));
}

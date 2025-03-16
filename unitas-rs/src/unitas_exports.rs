use std::time::Duration;

use log::error;

// use crate::{hook::hooks, state::os::time::TIME};

#[unsafe(no_mangle)]
pub extern "C" fn last_update_set_callback(
    callback: hooks::unity::player_loop::LastUpdateCallbackFn,
) {
    unsafe { hooks::unity::player_loop::LAST_UPDATE_CALLBACK = Some(callback) };
}

#[unsafe(no_mangle)]
pub extern "C" fn init() {
    crate::init();
}

#[unsafe(no_mangle)]
pub extern "C" fn update_actual(delta_time: f64) {
    // TIME.lock()
    //     .unwrap()
    //     .add_frame_time(Duration::from_secs_f64(delta_time));
}

#[unsafe(no_mangle)]
pub extern "C" fn restart(secs: u64, nano_secs: u32) -> bool {
    // if let Err(err) = TIME.lock().unwrap().restart(Duration::new(secs, nano_secs)) {
    //     error!("restart: {err}");
    //     return false;
    // }

    true
}

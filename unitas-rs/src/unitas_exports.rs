use crate::hook::hooks;

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

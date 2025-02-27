use crate::hooks::{self, LastUpdateCallbackFn};

#[unsafe(no_mangle)]
pub extern "C" fn last_update_set_callback(callback: LastUpdateCallbackFn) {
    unsafe { hooks::LAST_UPDATE_CALLBACK = Some(callback) };
}

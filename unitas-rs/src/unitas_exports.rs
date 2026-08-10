use std::{
    ffi::{CStr, c_char},
    path::Path,
    slice,
};

use crate::{hook::hooks, state::fs::FS};

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

// #[unsafe(no_mangle)]
// pub extern "C" fn update_actual(_delta_time: f64) {
//     TIME.lock()
//         .unwrap()
//         .add_frame_time(Duration::from_secs_f64(delta_time));
// }

// #[unsafe(no_mangle)]
// pub extern "C" fn restart(_secs: u64, _nano_secs: u32) -> bool {
//     if let Err(err) = TIME.lock().unwrap().restart(Duration::new(secs, nano_secs)) {
//         error!("restart: {err}");
//         return false;
//     }
//
//     true
// }

#[unsafe(no_mangle)]
/// Call as movie is entering startup
///
/// # Arguments
/// - `fs_path`: Path of movie filesystem.
/// - `fs_passthrough`: Array of venv fs to passthrough. Set the count through `fs_passthrough_count`
pub extern "C" fn movie_start(
    fs_path: *const c_char,
    fs_passthrough: *const *const c_char,
    fs_passthrough_count: usize,
) {
    let fs_path = if fs_path.is_null() {
        None
    } else {
        let fs_path = unsafe { CStr::from_ptr(fs_path) };
        let fs_path = fs_path.to_str().expect("movie path string is not valid");
        Some(Path::new(fs_path))
    };

    let fs_passthrough = unsafe { slice::from_raw_parts(fs_passthrough, fs_passthrough_count) };
    let fs_passthrough = fs_passthrough
        .iter()
        .map(|x| unsafe { CStr::from_ptr(*x) })
        .map(|x| {
            x.to_str()
                .expect("fs_passthrough path is not a valid string")
        })
        .map(Path::new)
        .collect::<Vec<_>>();

    FS.lock().unwrap().movie_start(fs_path, &fs_passthrough);
}

#[unsafe(no_mangle)]
pub extern "C" fn movie_end() {
    FS.lock().unwrap().movie_end();
}

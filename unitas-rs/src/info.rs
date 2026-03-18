use std::{
    sync::{LazyLock, Mutex},
    thread::{self, ThreadId},
};

use log::debug;

static MAIN_THREAD_ID: LazyLock<Mutex<ThreadId>> =
    LazyLock::new(|| Mutex::new(thread::current().id()));

pub fn set_main_thread() {
    let mut prev_id = MAIN_THREAD_ID.lock().unwrap();
    let current_id = thread::current().id();
    if *prev_id != current_id {
        debug!("new thread ID, {current_id:?}");
        *prev_id = current_id;
    }
}

pub fn is_main_thread() -> bool {
    *MAIN_THREAD_ID.lock().unwrap() == thread::current().id()
}

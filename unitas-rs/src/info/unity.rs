use std::{
    sync::OnceLock,
    thread::{self, ThreadId},
};

pub static MAIN_THREAD_ID: OnceLock<ThreadId> = const { OnceLock::new() };

pub fn is_main_thread() -> bool {
    *MAIN_THREAD_ID
        .get()
        .expect("the main thread ID is not set, this shouldn't happen")
        == thread::current().id()
}

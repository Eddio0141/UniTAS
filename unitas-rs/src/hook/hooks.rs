use std::{cell::Cell, marker::PhantomData};

#[cfg(unix)]
pub mod libc;

pub mod unity;

thread_local! {
    /// Controls if the original function for detours should be used instead
    static REVERSE_INVOKE: Cell<bool> = const { Cell::new(false) };
}

/// Thread local detour reverse invoke marker
///
/// Using this will let you call any functions with its original behaviour, despite having detours enabled
/// This is local to a thread, and there is no alternative to allow this
///
/// # Usage
/// Use [ReverseInvoke::new()]
/// Calling the function will activate reverse invoking for all detours in the current thread
///
/// To stop the reverse invoker, drop the instance
pub struct ReverseInvoke {
    // !Send and !Sync
    _marker: PhantomData<*const ()>,
}

impl ReverseInvoke {
    #[must_use]
    pub fn new() -> Self {
        REVERSE_INVOKE.set(true);
        Self {
            _marker: PhantomData,
        }
    }
}

impl Drop for ReverseInvoke {
    fn drop(&mut self) {
        REVERSE_INVOKE.set(false);
    }
}

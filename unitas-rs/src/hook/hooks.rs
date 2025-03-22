use std::{cell::Cell, marker::PhantomData};

#[cfg(unix)]
pub mod libc;

#[cfg(windows)]
pub mod win32;

pub mod unity;

thread_local! {
    /// Controls if the original function for detours should be used instead
    static REVERSE_INVOKE: Cell<bool> = const { Cell::new(false) };
}

#[unsafe(no_mangle)]
/// # Safety
/// This function isn't meant to be used in the crate, it exists for UniTAS C# side of things
unsafe extern "C" fn toggle_reverse_invoker(enable: bool) {
    REVERSE_INVOKE.set(enable);
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

#[macro_export]
/// Enables reverse invoker for this scope
///
/// # Note
/// All it is doing is a [ReverseInvoke::new()] invoke assigned to a variable
macro_rules! reverse_invoke {
    () => {
        let _ri = $crate::hook::hooks::ReverseInvoke::new();
    };
}

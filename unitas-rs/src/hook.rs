use installer::Hook;

pub mod hooks;
mod installer;

pub use installer::install;

const HOOKS: &[Hook] = &[hooks::unity::player_loop::last_update_hook()];

fn install_detours() {
    #[cfg(unix)]
    hooks::libc::install_detours();
}

#[macro_export]
macro_rules! detour_setup_log_fail {
    ($detour: expr, $before: expr, $after: expr) => {
        let detour = &$detour;
        let before = $before;
        let after = $after;
        if let Err(err) = unsafe { detour.initialize(before, after) } {
            log::warn!(
                "detour: failed to init `{}`, reason: {err:?}",
                stringify!($detour)
            );
        }
        if let Err(err) = unsafe { detour.enable() } {
            log::warn!(
                "detour: failed to enable `{}`, reason: {err:?}",
                stringify!($detour)
            );
        }
    };
}

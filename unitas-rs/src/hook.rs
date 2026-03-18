use log::info;

use crate::memory;

pub mod hooks;

pub fn install() {
    let search = &memory::SEARCH;

    info!("installing hooks");

    info!("last_update_hook");
    hooks::unity::player_loop::last_update_hook(search);

    // #[cfg(windows)]
    // hooks::win32::time::install_detours();

    info!("installed hooks");
}

#[macro_export]
macro_rules! detour_setup_log_fail {
    ($detour: expr, $before: expr, $signiture: ty, $after: expr) => {
        let detour = &$detour;
        let before = $before;
        let after = $after;
        if let Err(err) = unsafe {
            detour.initialize(
                std::mem::transmute::<*const (), $signiture>(before as *const ()),
                after,
            )
        } {
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

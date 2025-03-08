use installer::Hook;

pub mod hooks;
mod installer;

pub use installer::install;

const HOOKS: &[Hook] = &[hooks::unity::player_loop::last_update_hook()];

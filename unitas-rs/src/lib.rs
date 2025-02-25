use log::*;

mod hooks;
mod memory;

#[unsafe(no_mangle)]
pub extern "C" fn init() {
    env_logger::Builder::from_env(env_logger::Env::default().default_filter_or("info")).init();

    info!("initilising unitas-rs");

    hooks::install();

    info!("initialised unitas-rs");
}

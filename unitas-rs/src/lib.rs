use log::info;

mod hook;
mod memory;
mod unitas_exports;

#[unsafe(no_mangle)]
pub extern "C" fn init() {
    env_logger::Builder::from_env(env_logger::Env::default().default_filter_or("info"))
        .target(env_logger::Target::Stdout)
        .init();

    info!("initilising unitas-rs");

    hook::install();

    info!("initialised unitas-rs");
}

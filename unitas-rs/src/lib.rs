use log::info;

mod hooks;
mod memory;

#[unsafe(no_mangle)]
pub extern "C" fn init() {
    // TODO: fix logging
    // env_logger::builder()
    //     .target(env_logger::Target::Stdout)
    //     .init();

    info!("initilising unitas-rs");

    hooks::install();

    info!("initialised unitas-rs");
}

use std::{
    env,
    panic::{self, PanicHookInfo},
    sync::OnceLock,
    thread,
};

use backtrace::Backtrace;
use log::{LevelFilter, info};
use logger::DiskLogger;

mod error;
mod hook;
pub mod info;
mod logger;
mod memory;
mod state;
mod unitas_exports;

fn init() {
    init_logger();

    info!("initilising unitas-rs");

    info::unity::MAIN_THREAD_ID
        .set(thread::current().id())
        .unwrap();

    hook::install();

    info!("initialised unitas-rs");
}

static LOGGER: OnceLock<DiskLogger> = OnceLock::new();

fn init_logger() {
    // TODO: Debug for now
    let default_lvl = LevelFilter::Debug;
    let max_level = match env::var("UNITAS_LOG") {
        Ok(lvl) => lvl.parse::<LevelFilter>().unwrap_or(default_lvl),
        Err(_) => default_lvl,
    };

    let logger = LOGGER.get_or_init(|| DiskLogger::new().expect("failed to initialize DiskLogger"));
    log::set_logger(logger).expect("failed to initialize logger");
    log::set_max_level(max_level);

    panic::set_hook(Box::new(panic_hook));
}

fn panic_hook(info: &PanicHookInfo<'_>) {
    let thread = thread::current();
    let thread = thread.name().unwrap_or("unnamed");
    let payload = if let Some(s) = info.payload().downcast_ref::<&str>() {
        s
    } else if let Some(s) = info.payload().downcast_ref::<String>() {
        s
    } else {
        ""
    };
    let loc = match info.location() {
        Some(loc) => format!("{}:{}:{}", loc.file(), loc.line(), loc.column()),
        None => "<unknown>".to_owned(),
    };
    let bt = format!("\nstack backtrace:\n{:?}", Backtrace::new());
    log::error!("thread '{thread}' panicked at {loc}:\n{payload}{bt}");

    log::logger().flush();
}

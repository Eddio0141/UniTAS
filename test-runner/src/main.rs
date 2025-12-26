use crate::unitas_tests::{LINUX_TESTS, WIN_TESTS};
use anyhow::{Result, bail};
use clap::Parser;
use cli::Args;
use std::env;

mod cli;
mod movies;
mod symbols;
mod unitas_tests;

fn main() -> Result<()> {
    env_logger::init();

    let args = Args::parse();
    if !args.path.is_file() {
        bail!("game executable path is not a file");
    }

    let tests = match env::consts::OS {
        "linux" => LINUX_TESTS,
        "windows" => WIN_TESTS,
        _ => bail!("unsupported os for testing"),
    };

    // find matching test
    let Some(name) = args.path.parent() else {
        bail!("argument executable doesn't have a parent directory");
    };
    let name = name.file_stem().unwrap();
    let Some(test) = tests.iter().find(|t| t.name == name) else {
        bail!("test was not found");
    };

    test.run(&args)?;

    Ok(())
}

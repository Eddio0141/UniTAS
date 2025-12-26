use clap::Parser;
use std::path::PathBuf;

#[derive(Parser)]
#[command(version, about)]
pub struct Args {
    #[arg(long, default_value_t = 8080)]
    /// Port to use for the TCP connection between this tool and UniTAS
    pub port: u16,

    /// Game executable path
    pub path: PathBuf,
}

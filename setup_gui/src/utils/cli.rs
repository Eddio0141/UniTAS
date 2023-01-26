use std::path::PathBuf;

use clap::{command, Parser, Subcommand};

#[derive(Parser)]
#[command(author, version, about)]
pub struct Cli {
    #[command(subcommand)]
    pub command: Command,
}

#[derive(Subcommand)]
pub enum Command {
    GetInfo {
        game_dir_selection: GameDirSelection,
    },
    Install {
        game_dir_selection: GameDirSelection,
        unitas_version: VersionSelection,
        bepinex_version: VersionSelection,
        offline: bool,
    },
    Uninstall {
        game_dir_selection: GameDirSelection,
        remove_bepinex: bool,
        offline: bool,
    },
    LocalUniTASVersions,
    LocalBepInExVersions,
    GameDirHistory,
}

#[derive(Clone)]
pub enum GameDirSelection {
    Path(PathBuf),
    History(usize),
}

pub enum VersionSelection {
    Latest,
    SemVer(SemVer),
}

pub struct SemVer {
    pub major: u32,
    pub minor: u32,
    pub patch: u32,
}

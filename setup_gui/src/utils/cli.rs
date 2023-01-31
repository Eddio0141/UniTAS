use std::{path::PathBuf, str::FromStr};

use clap::{command, Parser, Subcommand};
use semver::Version;

use super::{local_versions::LocalVersions, paths};

#[derive(Parser)]
#[command(author, version, about)]
pub struct Cli {
    #[command(subcommand)]
    pub command: Command,
}

impl Cli {
    pub fn process(&self) -> crate::prelude::Result<()> {
        match &self.command {
            Command::GetInfo { game_dir_selection } => todo!(),
            Command::Install {
                game_dir_selection,
                unitas_version,
                bepinex_version,
                offline,
            } => todo!(),
            Command::Uninstall {
                game_dir_selection,
                remove_bepinex,
            } => todo!(),
            Command::LocalUniTASVersions => println!(
                "Locally available UniTAS versions\n{}",
                LocalVersions::from_dir(&paths::unitas_dir()?)?
            ),
            Command::LocalBepInExVersions => println!(
                "Locally available BepInEx versions\n{}",
                LocalVersions::from_dir(&paths::bepinex_dir()?)?
            ),
            Command::GameDirHistory => todo!(),
        }

        Ok(())
    }
}

#[derive(Subcommand)]
pub enum Command {
    GetInfo {
        #[arg(short, long)]
        game_dir_selection: GameDirSelection,
    },
    Install {
        #[arg(short, long)]
        game_dir_selection: GameDirSelection,
        #[arg(short, long)]
        unitas_version: VersionSelection,
        #[arg(short, long)]
        bepinex_version: VersionSelection,
        #[arg(short, long)]
        offline: bool,
    },
    Uninstall {
        #[arg(short, long)]
        game_dir_selection: GameDirSelection,
        remove_bepinex: bool,
    },
    #[command(name = "local-unitas-versions")]
    LocalUniTASVersions,
    #[command(name = "local-bepinex-versions")]
    LocalBepInExVersions,
    GameDirHistory,
}

#[derive(Clone)]
pub enum GameDirSelection {
    Path(PathBuf),
    History(usize),
}

impl From<&str> for GameDirSelection {
    fn from(value: &str) -> Self {
        if let Ok(index) = value.parse::<usize>() {
            Self::History(index)
        } else {
            Self::Path(PathBuf::from(value))
        }
    }
}

impl TryFrom<GameDirSelection> for PathBuf {
    type Error = crate::error::Error;

    fn try_from(value: GameDirSelection) -> Result<Self, Self::Error> {
        match value {
            GameDirSelection::Path(path) => Ok(path),
            GameDirSelection::History(index) => {
                let history = crate::utils::history::History::load()?;
                let path = history.index(index)?;
                Ok(path.to_path_buf())
            }
        }
    }
}

#[derive(Clone)]
pub enum VersionSelection {
    Stable,
    Master,
    SemVer(Version),
}

impl FromStr for VersionSelection {
    type Err = semver::Error;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        let result = match s.to_lowercase().as_str() {
            "master" => Self::Master,
            "stable" => Self::Stable,
            _ => Self::SemVer(Version::from_str(s)?),
        };

        Ok(result)
    }
}

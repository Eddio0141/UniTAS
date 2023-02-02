use std::{fmt::Display, path::PathBuf, str::FromStr};

use clap::{command, Parser, Subcommand};
use semver::Version;

use super::{download, game_dir::dir_info::DirInfo, local_versions::LocalVersions, paths};

#[derive(Parser)]
#[command(author, version, about)]
pub struct Cli {
    #[command(subcommand)]
    pub command: Command,
}

impl Cli {
    pub async fn process(&self) -> crate::prelude::Result<()> {
        match &self.command {
            Command::GetInfo { game_dir_selection } => {
                let dir_info =
                    DirInfo::from_dir(PathBuf::try_from(game_dir_selection.clone())?.as_path())?;
                println!("{}", dir_info);
            }
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
            Command::LocalUniTAS => println!(
                "Locally available UniTAS versions\n{}",
                LocalVersions::from_dir(&paths::unitas_dir()?)?
            ),
            Command::LocalBepInEx => println!(
                "Locally available BepInEx versions\n{}",
                LocalVersions::from_dir(&paths::bepinex_dir()?)?
            ),
            Command::GameDirHistory => todo!(),
            Command::DownloadUniTAS { version } => {
                let dest_path = download::download_unitas(version).await?;
                println!("Downloaded UniTAS to {}", dest_path.display());
            }
            Command::DownloadBepInEx { version } => todo!(),
        }

        Ok(())
    }
}

#[derive(Subcommand)]
pub enum Command {
    GetInfo {
        game_dir_selection: GameDirSelection,
    },
    Install {
        game_dir_selection: GameDirSelection,
        #[arg(default_value = "stable")]
        unitas_version: DownloadVersion,
        #[arg(default_value = "stable")]
        bepinex_version: DownloadVersion,
        #[arg(short, long)]
        offline: bool,
    },
    Uninstall {
        game_dir_selection: GameDirSelection,
        #[arg(short, long)]
        remove_bepinex: bool,
    },
    #[command(name = "local-unitas")]
    LocalUniTAS,
    #[command(name = "local-bepinex")]
    LocalBepInEx,
    GameDirHistory,
    #[command(name = "download-unitas")]
    DownloadUniTAS {
        version: DownloadVersion,
    },
    #[command(name = "download-bepinex")]
    DownloadBepInEx {
        version: DownloadVersion,
    },
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
                let entry = history.index(index)?;
                Ok(entry.to_path_buf())
            }
        }
    }
}

#[derive(Clone, PartialEq, Eq, PartialOrd, Ord)]
pub enum DownloadVersion {
    Stable,
    Branch(String),
    SemVer(Version),
}

impl From<&str> for DownloadVersion {
    fn from(value: &str) -> Self {
        if let "stable" = value.to_lowercase().as_str() {
            Self::Stable
        } else if let Ok(sem_ver) = Version::from_str(value) {
            Self::SemVer(sem_ver)
        } else {
            Self::Branch(value.to_string())
        }
    }
}

impl FromStr for DownloadVersion {
    type Err = crate::error::Error;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        Ok(Self::from(s))
    }
}

impl Display for DownloadVersion {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Stable => write!(f, "stable"),
            Self::Branch(branch) => write!(f, "{branch}"),
            Self::SemVer(sem_ver) => write!(f, "{sem_ver}"),
        }
    }
}

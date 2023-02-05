use std::{
    fmt::Display,
    path::{Path, PathBuf},
};

use anyhow::Context;
use clap::{command, Args, Parser, Subcommand};
use log::*;

use crate::utils::uninstall;

use super::{
    download,
    game_dir::dir_info::DirInfo,
    history::{self, History},
    install,
    local_versions::LocalVersions,
    paths,
};

#[derive(Parser)]
#[command(author, version, about)]
pub struct Cli {
    #[command(subcommand)]
    pub command: Command,
}

impl Cli {
    async fn save_path_history(game_dir_selection: &GameDirSelection) -> anyhow::Result<()> {
        let mut history = History::load().context("Failed to load game dir history")?;
        history.add(game_dir_selection.clone().try_into()?);
        history.save().context("Failed to save game dir history")?;
        Ok(())
    }

    pub async fn process(&self) -> crate::prelude::Result<()> {
        match &self.command {
            Command::GetInfo { game_dir_selection } => {
                info!("Getting info for {game_dir_selection}");
                let dir_info =
                    DirInfo::from_dir(PathBuf::try_from(game_dir_selection.clone())?.as_path())?;
                info!("{dir_info}");

                Cli::save_path_history(game_dir_selection).await?;
            }
            Command::Install {
                game_dir_selection,
                unitas_version,
                bepinex_version,
                offline,
            } => {
                info!("Installing UniTAS {unitas_version} and BepInEx {} to {game_dir_selection} with offline = {offline}", DownloadVersionArg::from(bepinex_version));
                install::install(
                    game_dir_selection.clone(),
                    unitas_version.into(),
                    DownloadVersionArg::from(bepinex_version).into(),
                    *offline,
                )
                .await?;
                info!("Installed everything successfully");

                Cli::save_path_history(game_dir_selection).await?;
            }
            Command::Uninstall { game_dir_selection } => {
                info!("Uninstalling UniTAS from {game_dir_selection}");
                uninstall::uninstall(game_dir_selection).await?;
                info!("Uninstalled successfully");

                Cli::save_path_history(game_dir_selection).await?;
            }
            Command::LocalUniTAS => info!(
                "Locally available UniTAS versions\n{}",
                LocalVersions::from_dir(&paths::unitas_dir()?)?
            ),
            Command::LocalBepInEx => info!(
                "Locally available BepInEx versions\n{}",
                LocalVersions::from_dir(&paths::bepinex_dir()?)?
            ),
            Command::History => {
                let history = History::load().context("Failed to load game dir history")?;
                info!("Game dir history\n{}", history);
            }
            Command::DownloadUniTAS { version } => {
                info!("Downloading UniTAS {}", version);
                download::download_unitas(&version.into()).await?;
                info!("Finished downloading");
            }
            Command::DownloadBepInEx { version } => {
                info!("Downloading BepInEx {}", version);
                download::download_bepinex(&version.into()).await?;
                info!("Finished downloading");
            }
        }

        Ok(())
    }
}

#[derive(Subcommand)]
pub enum Command {
    /// Gets installed info about UniTAS and BepInEx
    GetInfo {
        game_dir_selection: GameDirSelection,
    },
    /// Installs UniTAS and BepInEx to the specified game directory
    Install {
        game_dir_selection: GameDirSelection,
        #[command(flatten)]
        unitas_version: DownloadVersionArg,
        #[command(flatten)]
        bepinex_version: DownloadVersionArgBepInEx,
        #[arg(short, long)]
        /// Installs without downloading, will fail if the specified versions are not already downloaded
        offline: bool,
    },
    /// Uninstalls UniTAS from the specified game directory
    Uninstall {
        game_dir_selection: GameDirSelection,
    },
    #[command(name = "local-unitas")]
    /// Lists locally available UniTAS versions
    LocalUniTAS,
    #[command(name = "local-bepinex")]
    /// Lists locally available BepInEx versions
    LocalBepInEx,
    /// Lists the game directory history
    History,
    #[command(name = "download-unitas")]
    /// Downloads a UniTAS version
    DownloadUniTAS {
        #[command(flatten)]
        version: DownloadVersionArg,
    },
    #[command(name = "download-bepinex")]
    /// Downloads a BepInEx version
    DownloadBepInEx {
        #[command(flatten)]
        version: DownloadVersionArg,
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
    type Error = history::error::Error;

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

impl Display for GameDirSelection {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            GameDirSelection::Path(path) => write!(f, "{}", path.display()),
            GameDirSelection::History(index) => {
                if let Ok(path) = PathBuf::try_from(self.clone()) {
                    write!(f, "{}", path.display())
                } else {
                    write!(f, "history entry {index}")
                }
            }
        }
    }
}

#[derive(Args, Clone)]
/// Download a specific version
/// If no version is specified, the latest stable release will be downloaded
pub struct DownloadVersionArg {
    #[arg(long, required_unless_present = "branch")]
    /// Download a specific release by tag
    pub tag: Option<String>,
    #[arg(long, required_unless_present = "tag")]
    /// Download a nightly build from a specific branch
    pub branch: Option<String>,
}

#[derive(Args, Clone)]
/// Download a specific version
/// If no version is specified, the latest stable release will be downloaded
pub struct DownloadVersionArgBepInEx {
    #[arg(long, required_unless_present = "bepinex_branch")]
    /// Download a specific release by tag
    pub bepinex_tag: Option<String>,
    #[arg(long, required_unless_present = "bepinex_tag")]
    /// Download a nightly build from a specific branch
    pub bepinex_branch: Option<String>,
}

impl From<&DownloadVersionArgBepInEx> for DownloadVersionArg {
    fn from(value: &DownloadVersionArgBepInEx) -> Self {
        Self {
            tag: value.bepinex_tag.to_owned(),
            branch: value.bepinex_branch.to_owned(),
        }
    }
}

impl Default for DownloadVersionArg {
    fn default() -> Self {
        Self {
            tag: None,
            branch: None,
        }
    }
}

impl Display for DownloadVersionArg {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        if let Some(tag) = &self.tag {
            write!(f, "tag {tag}")
        } else if let Some(branch) = &self.branch {
            write!(f, "branch {branch}")
        } else {
            write!(f, "stable")
        }
    }
}

impl From<&DownloadVersionArg> for DownloadVersion {
    fn from(value: &DownloadVersionArg) -> Self {
        if let Some(tag) = &value.tag {
            Self::Tag(tag.clone())
        } else if let Some(branch) = &value.branch {
            Self::Branch(branch.clone())
        } else {
            Self::Stable
        }
    }
}

impl From<DownloadVersionArg> for DownloadVersion {
    fn from(value: DownloadVersionArg) -> Self {
        if let Some(tag) = value.tag {
            Self::Tag(tag)
        } else if let Some(branch) = value.branch {
            Self::Branch(branch)
        } else {
            Self::Stable
        }
    }
}

#[derive(Clone, PartialEq, Eq, PartialOrd, Ord)]
pub enum DownloadVersion {
    Stable,
    Tag(String),
    Branch(String),
}

impl From<&DownloadVersion> for PathBuf {
    fn from(value: &DownloadVersion) -> Self {
        match value {
            DownloadVersion::Stable => PathBuf::from("stable"),
            DownloadVersion::Tag(tag) => Path::new("tag").join(PathBuf::from(tag)),
            DownloadVersion::Branch(branch) => {
                let branch = branch.replace('/', "-");
                Path::new("branch").join(PathBuf::from(branch))
            }
        }
    }
}

impl Display for DownloadVersion {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Stable => write!(f, "stable"),
            Self::Tag(tag) => write!(f, "tag {tag}"),
            Self::Branch(branch) => write!(f, "branch {branch}"),
        }
    }
}

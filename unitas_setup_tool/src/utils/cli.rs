use std::{
    fmt::Display,
    path::{Path, PathBuf},
};

use clap::{command, Args, Parser, Subcommand};

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
                let dest_path = download::download_unitas(&version.into()).await?;
                println!("Downloaded UniTAS to {}", dest_path.display());
            }
            Command::DownloadBepInEx { version } => {
                let dest_path = download::download_bepinex(&version.into()).await?;
                println!("Downloaded BepInEx to {}", dest_path.display());
            }
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
        #[command(flatten)]
        unitas_version: DownloadVersionArg,
        #[command(flatten)]
        bepinex_version: DownloadVersionArg,
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
        #[command(flatten)]
        version: DownloadVersionArg,
    },
    #[command(name = "download-bepinex")]
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

#[derive(Args, Clone)]
pub struct DownloadVersionArg {
    #[arg(long, required_unless_present_any = &["tag", "branch"])]
    pub stable: bool,
    #[arg(long, required_unless_present_any = &["stable", "branch"])]
    pub tag: Option<String>,
    #[arg(long, required_unless_present_any = &["stable", "tag"])]
    pub branch: Option<String>,
}

impl Default for DownloadVersionArg {
    fn default() -> Self {
        Self {
            stable: true,
            tag: None,
            branch: None,
        }
    }
}

impl Display for DownloadVersionArg {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        if self.stable {
            write!(f, "stable")
        } else if let Some(tag) = &self.tag {
            write!(f, "tag {}", tag)
        } else if let Some(branch) = &self.branch {
            write!(f, "branch {}", branch)
        } else {
            unreachable!()
        }
    }
}

impl From<&DownloadVersionArg> for DownloadVersion {
    fn from(value: &DownloadVersionArg) -> Self {
        if value.stable {
            Self::Stable
        } else if let Some(tag) = &value.tag {
            Self::Tag(tag.clone())
        } else if let Some(branch) = &value.branch {
            Self::Branch(branch.clone())
        } else {
            unreachable!()
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

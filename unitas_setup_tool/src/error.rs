//! Main errors

use crate::utils::*;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error("Semver error: {0}")]
    Semver(#[from] semver::Error),
    #[error(transparent)]
    PathError(#[from] paths::error::Error),
    #[error(transparent)]
    HistoryError(#[from] history::error::Error),
    #[error(transparent)]
    GameDirError(#[from] game_dir::error::Error),
    #[error("Failed to download UniTAS: {0}")]
    DownloadUniTASError(#[from] download::error::Error),
    #[error(transparent)]
    LocalVersionsError(#[from] local_versions::Error),
    #[error(transparent)]
    InstallError(#[from] install::Error),
}

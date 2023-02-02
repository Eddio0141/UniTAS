//! Main errors

use crate::utils::{history, paths};

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
}

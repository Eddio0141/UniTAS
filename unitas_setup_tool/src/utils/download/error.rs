use tokio::task::JoinError;

use crate::utils::{github_artifact, paths};

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Failed to get latest branch build: {0}")]
    GithubArtifactError(#[from] github_artifact::error::Error),
    #[error("Failed to join thread: {0}")]
    JoinError(#[from] JoinError),
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error("Request error: {0}")]
    RequestError(#[from] reqwest::Error),
    #[error(transparent)]
    PathsError(#[from] paths::error::Error),
    #[error("Failed to unzip file: {0}")]
    UnzipError(#[from] zip::result::ZipError),
}

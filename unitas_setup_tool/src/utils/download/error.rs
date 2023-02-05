use tokio::task::JoinError;

use crate::utils::{github_artifact, paths};

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Failed to get latest branch build: {0}")]
    GithubArtifact(#[from] github_artifact::error::Error),
    #[error("Failed to join thread: {0}")]
    Join(#[from] JoinError),
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error("Request error: {0}")]
    Request(#[from] reqwest::Error),
    #[error(transparent)]
    Paths(#[from] paths::error::Error),
    #[error("Failed to unzip file: {0}")]
    Unzip(#[from] zip::result::ZipError),
    #[error("Empty download folder")]
    EmptyDownloadFolder,
}

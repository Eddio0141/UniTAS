use crate::utils::github_artifact;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Failed to get latest branch build: {0}")]
    GithubArtifactError(#[from] github_artifact::error::Error),
}

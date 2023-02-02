#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Can't find artifacts url from json data")]
    CantFindArtifactsUrl,
    #[error("Unexpected artifacts json data")]
    UnexpectedArtifactsJsonData,
    #[error("Can't find workflow runs from json data")]
    CantFindWorkflowRuns,
    #[error("Request error: {0}")]
    RequestError(#[from] reqwest::Error),
    #[error("Json parse error: {error}, response: {response}")]
    JsonParseError {
        #[source]
        error: serde_json::Error,
        response: String,
    },
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error("Unzip error: {0}")]
    UnzipError(#[from] zip::result::ZipError),
    #[error("Thread join error: {0}")]
    JoinError(#[from] tokio::task::JoinError),
}

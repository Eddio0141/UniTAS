use crate::utils::paths;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error(transparent)]
    PathError(#[from] paths::error::Error),
    #[error("Serde json error: {0}")]
    SerdeJson(#[from] serde_json::Error),
    #[error("History index out of range: {index} (history size: {history_size})")]
    HistoryIndexOutOfRange { index: usize, history_size: usize },
}

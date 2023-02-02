use std::io;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Not an unity game directory")]
    NotUnityGameDir,
    #[error("IO error: {0}")]
    Io(#[from] io::Error),
}

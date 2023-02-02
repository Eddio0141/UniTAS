use std::io;

use crate::utils::dll_file_version;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Not an unity game directory")]
    NotUnityGameDir,
    #[error("IO error: {0}")]
    Io(#[from] io::Error),
    #[error("Can't get BepInEx dll version")]
    BepInExVersionNotFound,
    #[error(transparent)]
    DllFileVersionError(#[from] dll_file_version::error::Error),
}

use crate::utils::assembly_version;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("IO error: {0}")]
    IoError(#[from] std::io::Error),
    #[error("Can't open PE file: {0}")]
    PeFileError(#[from] pelite::Error),
    #[error("Can't find resource: {0}")]
    ResourceError(#[from] pelite::resources::FindError),
    #[error("Can't find file assembly version")]
    CantFindFileVersion,
    #[error(transparent)]
    AssemblyVersionError(#[from] assembly_version::error::Error),
}

use crate::utils::assembly_version;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error("Can't open PE file: {0}")]
    PeFile(#[from] pelite::Error),
    #[error("Can't find resource: {0}")]
    Resource(#[from] pelite::resources::FindError),
    #[error("Can't find file assembly version")]
    CantFindFileVersion,
    #[error(transparent)]
    AssemblyVersion(#[from] assembly_version::error::Error),
}

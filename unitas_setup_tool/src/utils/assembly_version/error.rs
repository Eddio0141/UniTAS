use std::num::ParseIntError;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Int parse error: {0}")]
    IntParseError(#[from] ParseIntError),
    #[error("Invalid assembly version string: {0}")]
    InvalidAssemblyVersionString(String),
}

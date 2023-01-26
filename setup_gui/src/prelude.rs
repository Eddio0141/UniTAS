//! Crate prelude.

pub use crate::error::Error;

pub type Result<T> = std::result::Result<T, Error>;

// Generic wrapper type
pub struct Wrap<T>(pub T);

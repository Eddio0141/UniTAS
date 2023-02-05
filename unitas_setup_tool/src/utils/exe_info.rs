use std::{
    fs::{self},
    path::Path,
};

use anyhow::{Context, Result};
use goblin::Object;

pub enum FileBitness {
    X86,
    X64,
    Other,
}

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
}

impl FileBitness {
    pub fn from_exe(path: &Path) -> Result<Self> {
        let bytes = fs::read(path).context("Failed to read file")?;

        let pe = Object::parse(&bytes).context("Failed to get PE info")?;

        let bitness = match pe {
            Object::PE(pe) => {
                if pe.is_64 {
                    FileBitness::X64
                } else {
                    FileBitness::X86
                }
            }
            _ => FileBitness::Other,
        };

        Ok(bitness)
    }
}

//! Get version info for semantically versioned dir names in a dir.

use std::{fmt::Display, path::Path, str::FromStr};

use super::cli::DownloadVersion;

pub struct LocalVersions {
    pub versions: Vec<DownloadVersion>,
}

impl LocalVersions {
    pub fn from_dir(dir: &Path) -> crate::prelude::Result<Self> {
        let mut versions = Vec::new();

        for entry in dir.read_dir()? {
            let path = entry?.path();

            if path.is_dir() {
                let Some(file_name) = path.file_name() else {
                    continue;
                };
                let Some(file_name) = file_name.to_str() else {
                    continue;
                };
                let version = DownloadVersion::from_str(file_name)?;
                versions.push(version);
            }
        }

        versions.sort();

        Ok(Self { versions })
    }
}

impl Display for LocalVersions {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        for version in &self.versions {
            writeln!(f, "{}", version)?;
        }

        Ok(())
    }
}

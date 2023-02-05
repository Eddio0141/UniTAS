//! Get version info for semantically versioned dir names in a dir.

use std::{fmt::Display, path::Path};

use super::{cli::DownloadVersion, paths};

pub struct LocalVersions {
    pub versions: Vec<DownloadVersion>,
}

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error(transparent)]
    PathsError(#[from] paths::error::Error),
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
}

impl LocalVersions {
    pub fn from_dir(dir: &Path) -> Result<Self, Error> {
        let mut versions = Vec::new();

        let stable_dir = paths::local_stable_path(dir);
        let tag_dir = paths::local_tag_path(dir);
        let branch_dir = paths::local_branch_path(dir);

        paths::create_dir_if_not_exists(&stable_dir)?;
        paths::create_dir_if_not_exists(&tag_dir)?;
        paths::create_dir_if_not_exists(&branch_dir)?;

        let entries = [
            stable_dir.read_dir()?,
            tag_dir.read_dir()?,
            branch_dir.read_dir()?,
        ];

        let version_handles = [
            |_: &str| DownloadVersion::Stable,
            |file_name: &str| DownloadVersion::Tag(file_name.to_string()),
            |file_name: &str| DownloadVersion::Branch(file_name.to_string()),
        ];

        for (entry, version_handle) in entries.into_iter().zip(version_handles.iter()) {
            for entry in entry {
                let path = entry?.path();

                if !path.is_dir() {
                    continue;
                }

                let Some(file_name) = path.file_name() else {
                            continue;
                        };
                let Some(file_name) = file_name.to_str() else {
                            continue;
                        };

                let version = version_handle(file_name);
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
            writeln!(f, "{version}")?;
        }

        Ok(())
    }
}

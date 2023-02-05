pub mod error;

use std::{fmt::Display, str::FromStr};

pub struct AssemblyVersion {
    pub major: u32,
    pub minor: u32,
    pub build: u32,
    pub revision: u32,
}

impl FromStr for AssemblyVersion {
    type Err = error::Error;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        let mut parts = s.split('.');

        let (Some(major), Some(minor), Some(build), Some(revision)) = (parts.next(), parts.next(), parts.next(), parts.next()) else {
            return Err(error::Error::InvalidAssemblyVersionString(s.to_string()));
        };

        if parts.next().is_some() {
            return Err(error::Error::InvalidAssemblyVersionString(s.to_string()));
        }

        let (major, minor, build, revision) = (
            major.parse::<u32>()?,
            minor.parse::<u32>()?,
            build.parse::<u32>()?,
            revision.parse::<u32>()?,
        );

        Ok(AssemblyVersion {
            major,
            minor,
            build,
            revision,
        })
    }
}

impl Display for AssemblyVersion {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "{}.{}.{}.{}",
            self.major, self.minor, self.build, self.revision
        )
    }
}

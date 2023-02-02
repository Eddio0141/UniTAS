pub mod error;

use std::{path::Path, str::FromStr};

use pelite::{FileMap, PeFile};

use crate::prelude::Wrap;

use super::assembly_version::AssemblyVersion;

impl TryFrom<Wrap<&Path>> for AssemblyVersion {
    type Error = error::Error;

    fn try_from(value: Wrap<&Path>) -> Result<Self, Self::Error> {
        let map = FileMap::open(value.0)?;
        let pe_file = PeFile::from_bytes(&map)?;

        let resources = pe_file.resources()?;
        let version_info = resources.version_info()?;

        // gets version via:
        // version_info -> fixed -> dwFileVersion
        // version_info -> fixed -> dwProductVersion
        // version_info -> strings -> ProductVersion
        // version_info -> strings -> FileVersion
        // version_info -> strings -> AssemblyVersion

        let mut version_info = match version_info.fixed() {
            // for now i just get dwFileVersion
            Some(fixed) => fixed.dwFileVersion.to_string(),
            None => 'version_string_return: {
                let mut version = None;

                let langs = version_info.translation();
                for lang in langs {
                    version_info.strings(*lang, |key, value| {
                        if key == "ProductVersion"
                            || key == "FileVersion"
                            || key == "AssemblyVersion"
                        {
                            version = Some(value.to_string());
                        }
                    });

                    if let Some(version) = version {
                        break 'version_string_return version;
                    }
                }

                return Err(error::Error::CantFindFileVersion);
            }
        };

        Ok(AssemblyVersion::from_str(&version_info)?)
    }
}

pub mod error;

use std::{fs, io::Cursor, path::PathBuf};

use zip::ZipArchive;

use self::error::Error;

use super::{cli::VersionSelection, github_artifact::Action, paths};

const UNITAS_OWNER: &str = "eddio0141";
const UNITAS_REPO: &str = "UniTAS";

const UNITAS_RELEASE: &str = "Release";
const UNIX_RELEASE: &str = "ubuntu-latest";
const WINDOWS_RELEASE: &str = "windows-latest";

/// Downloads UniTAS by version selection from GitHub
/// - `version` - The version to download
/// - returns the path to the downloaded UniTAS directory
pub async fn download_unitas(version: &VersionSelection) -> Result<PathBuf, Error> {
    let dest_path = paths::unitas_dir()?.join(version.to_string());

    match version {
        VersionSelection::Stable => todo!(),
        VersionSelection::Branch(branch) => {
            let mut action = Action::get_latest_action(UNITAS_OWNER, UNITAS_REPO, branch).await?;

            let windows_release = format!("{WINDOWS_RELEASE}-{UNITAS_RELEASE}");
            let unix_release = format!("{UNIX_RELEASE}-{UNITAS_RELEASE}");

            // for now we get unix and windows artifacts
            // remove from vec
            action.artifacts.retain(|artifact| {
                artifact.name == windows_release || artifact.name == unix_release
            });

            action.extract_to_dir(&dest_path).await?;
        }
        VersionSelection::SemVer(_) => todo!(),
    }

    Ok(dest_path)
}

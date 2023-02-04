pub mod error;

use std::{
    fs::{self, File},
    path::PathBuf,
};

use anyhow::Result;
use zip::ZipArchive;

use self::error::Error;

use super::{cli::DownloadVersion, github_artifact::Build, paths};

const UNITAS_OWNER: &str = "eddio0141";
const UNITAS_REPO: &str = "UniTAS";

const UNITAS_WORKFLOW_FILE_NAME: &str = "build-on-push.yml";

const UNITAS_RELEASE: &str = "Release";
const UNIX_RELEASE: &str = "ubuntu-latest";
const WINDOWS_RELEASE: &str = "windows-latest";

const BEPINEX_OWNER: &str = "BepInEx";
const BEPINEX_REPO: &str = "BepInEx";

const BEPINEX_WORKFLOW_FILE_NAME: &str = "build.yml";

/// Downloads UniTAS by version selection from GitHub
/// - `version` - The version to download
/// - returns the path to the downloaded UniTAS directory
pub async fn download_unitas(version: &DownloadVersion) -> Result<PathBuf, Error> {
    let dest_path = paths::unitas_dir()?.join(PathBuf::from(version));

    match version {
        DownloadVersion::Stable => {
            let build = Build::latest_stable_build(UNITAS_OWNER, UNITAS_REPO).await?;

            build.extract_to_dir(&dest_path).await?;
        }
        DownloadVersion::Branch(branch) => {
            let mut build = Build::latest_action_build(
                UNITAS_OWNER,
                UNITAS_REPO,
                branch,
                UNITAS_WORKFLOW_FILE_NAME,
            )
            .await?;

            let windows_release = format!("{WINDOWS_RELEASE}-{UNITAS_RELEASE}");
            let unix_release = format!("{UNIX_RELEASE}-{UNITAS_RELEASE}");

            // for now we get unix and windows artifacts
            // remove from vec
            build.artifacts.retain(|artifact| {
                artifact.name == windows_release || artifact.name == unix_release
            });

            build.extract_to_dir(&dest_path).await?;
        }
        DownloadVersion::Tag(_) => todo!(),
    }

    Ok(dest_path)
}

/// Downloads BepInEx by version selection from GitHub
/// - `version` - The version to download
/// - returns the path to the downloaded BepInEx directory
pub async fn download_bepinex(version: &DownloadVersion) -> Result<PathBuf, Error> {
    let dest_path = paths::bepinex_dir()?.join(PathBuf::from(version));

    match version {
        DownloadVersion::Stable => {
            let build = Build::latest_stable_build(BEPINEX_OWNER, BEPINEX_REPO).await?;

            build.extract_to_dir(&dest_path).await?;
        }
        DownloadVersion::Branch(branch) => {
            let action = Build::latest_action_build(
                BEPINEX_OWNER,
                BEPINEX_REPO,
                branch,
                BEPINEX_WORKFLOW_FILE_NAME,
            )
            .await?;

            action.extract_to_dir(&dest_path).await?;

            let inner_folder = fs::read_dir(&dest_path)?
                .next()
                .ok_or(Error::EmptyDownloadFolder)??
                .path();

            // because BepInEx has zip files inside zip files, we need to extract the inner zip
            let files = std::fs::read_dir(&inner_folder)?;
            let mut tasks = Vec::new();

            for file in files {
                let file = file?;
                let file_path = file.path();

                if let Some(extension) = file_path.extension() {
                    if extension != "zip" {
                        continue;
                    }
                } else {
                    continue;
                }

                let file_name_without_extension = file_path.file_stem().unwrap();

                let dest_path = dest_path.join(file_name_without_extension);
                tokio::fs::create_dir_all(&dest_path).await?;

                tasks.push(tokio::spawn(async move {
                    let mut archive = ZipArchive::new(File::open(&file_path)?)?;
                    archive.extract(dest_path)?;

                    // delete the zip file
                    fs::remove_file(file_path)?;
                    Ok::<_, Error>(())
                }));
            }

            for task in tasks {
                task.await??;
            }

            // delete the inner folder
            tokio::fs::remove_dir(inner_folder).await?;
        }
        DownloadVersion::Tag(_) => todo!(),
    }

    Ok(dest_path)
}

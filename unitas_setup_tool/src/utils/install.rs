use std::path::PathBuf;

use crate::utils::download;

use super::{
    cli::{DownloadVersion, GameDirSelection},
    game_dir::{self, dir_info::DirInfo},
    history,
    local_versions::{self, LocalVersions},
    paths,
};

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error(transparent)]
    DirInfoError(#[from] game_dir::error::Error),
    #[error(transparent)]
    HistoryLoadError(#[from] history::error::Error),
    #[error("Not an unity game dir")]
    NotUnityGameDir,
    #[error(transparent)]
    PathsError(#[from] paths::error::Error),
    #[error(transparent)]
    LocalVersionsError(#[from] local_versions::Error),
    #[error("Version not found, remove the --offline flag to download it")]
    VersionNotFound,
    #[error(transparent)]
    DownloadError(#[from] download::error::Error),
    #[error("Thread join error: {0}")]
    ThreadJoinError(#[from] tokio::task::JoinError),
}

pub async fn install(
    game_dir: GameDirSelection,
    unitas_version: DownloadVersion,
    bepinex_version: DownloadVersion,
    offline: bool,
) -> Result<(), Error> {
    let game_dir = PathBuf::try_from(game_dir)?;
    let dir_info = DirInfo::from_dir(&game_dir)?;

    if !dir_info.is_unity_dir {
        return Err(Error::NotUnityGameDir);
    }

    let installed_bepinex = LocalVersions::from_dir(&paths::bepinex_dir()?)?;
    let installed_unitas = LocalVersions::from_dir(&paths::unitas_dir()?)?;

    let mut download_tasks = Vec::new();

    if !installed_bepinex.versions.contains(&bepinex_version) {
        // download if not offline
        if offline {
            return Err(Error::VersionNotFound);
        }

        download_tasks.push(tokio::spawn(async move {
            let dest_path = download::download_bepinex(&bepinex_version.into()).await?;
            println!("Downloaded BepInEx to {}", dest_path.display());
            Ok::<_, download::error::Error>(())
        }));
    }

    if !installed_unitas.versions.contains(&unitas_version) {
        // download if not offline
        if offline {
            return Err(Error::VersionNotFound);
        }

        download_tasks.push(tokio::spawn(async move {
            let dest_path = download::download_unitas(&unitas_version.into()).await?;
            println!("Downloaded UniTAS to {}", dest_path.display());
            Ok::<_, download::error::Error>(())
        }));
    }

    for task in download_tasks {
        task.await??;
    }

    todo!()
}

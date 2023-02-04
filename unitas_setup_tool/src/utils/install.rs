use std::{
    fs,
    path::{Path, PathBuf},
};

use anyhow::{bail, Context, Result};
use log::*;

use crate::utils::download;

use super::{
    cli::{DownloadVersion, GameDirSelection},
    game_dir::dir_info::{DirInfo, GamePlatform},
    local_versions::LocalVersions,
    paths,
};

pub async fn install(
    game_dir: GameDirSelection,
    unitas_version: DownloadVersion,
    bepinex_version: DownloadVersion,
    offline: bool,
) -> Result<()> {
    let game_dir = PathBuf::try_from(game_dir)?;
    let dir_info = DirInfo::from_dir(&game_dir)?;

    if !dir_info.is_unity_dir {
        bail!("Game directory is not a Unity game directory");
    }

    if matches!(&dir_info.game_platform, GamePlatform::Unknown) {
        bail!("Game platform is unknown");
    }

    {
        let bepinex_version = bepinex_version.clone();
        let unitas_version = unitas_version.clone();

        let download_tasks = [
            tokio::spawn(async move { dl_bepinex_if_missing(bepinex_version, offline).await }),
            tokio::spawn(async move { dl_unitas_if_missing(unitas_version, offline).await }),
        ];

        for task in download_tasks {
            task.await.unwrap()?;
        }
    }

    install_bepinex(&game_dir, bepinex_version, &dir_info).await?;
    install_unitas(&game_dir, unitas_version).await?;

    info!("Installed UniTAS successfully");

    Ok(())
}

async fn dl_bepinex_if_missing(bepinex_version: DownloadVersion, offline: bool) -> Result<()> {
    let installed_bepinex = LocalVersions::from_dir(&paths::bepinex_dir()?)?;

    if !installed_bepinex.versions.contains(&bepinex_version) {
        // download if not offline
        if offline {
            bail!("BepInEx version not found and offline mode is enabled");
        }

        download::download_bepinex(&bepinex_version.into()).await?;
    }

    Ok(())
}

async fn dl_unitas_if_missing(unitas_version: DownloadVersion, offline: bool) -> Result<()> {
    let installed_unitas = LocalVersions::from_dir(&paths::unitas_dir()?)?;

    if !installed_unitas.versions.contains(&unitas_version) {
        // download if not offline
        if offline {
            bail!("UniTAS version not found and offline mode is enabled");
        }

        download::download_unitas(&unitas_version.into()).await?;
    }

    Ok(())
}

async fn install_bepinex(
    game_dir: &Path,
    bepinex_version: DownloadVersion,
    dir_info: &DirInfo,
) -> Result<()> {
    // overwrite install without overwriting the config and other important files
    let bepinex_dir = paths::bepinex_dir()?;
    let bepinex_dir = match bepinex_version {
        DownloadVersion::Stable => bepinex_dir.join(paths::STABLE_DIR_NAME),
        DownloadVersion::Tag(tag) => bepinex_dir.join(paths::TAG_DIR_NAME).join(tag),
        DownloadVersion::Branch(branch) => bepinex_dir.join(paths::BRANCH_DIR_NAME).join(branch),
    };
    let bepinex_dir = {
        // update this when we support IL2Cpp
        let files = fs::read_dir(bepinex_dir)?.collect::<Result<Vec<_>, _>>()?;

        let platform = match &dir_info.game_platform {
            GamePlatform::Unix => "unix",
            GamePlatform::Windowsx64 => "x64",
            GamePlatform::Windowsx86 => "x86",
            GamePlatform::Unknown => unreachable!(),
        };

        files
            .iter()
            .find_map(|entry| {
                let path = entry.path();
                let file_name = path.file_name()?.to_string_lossy();

                let exclusion = ["IL2CPP", "NetLauncher", platform];

                if path.is_file() || exclusion.contains(&file_name.as_ref()) {
                    return None;
                }

                Some(path)
            })
            .context("Could not find a BepInEx dir with the correct platform")
    }?;

    // path to overwrite and if it's a directory
    let overwrite_paths = [
        Path::new("changelog.txt"),
        Path::new("doorstop_libs"),
        Path::new("BepInEx/core"),
    ];

    let mut copy_tasks = Vec::new();
    for overwrite_path in overwrite_paths {
        let source_path = bepinex_dir.join(overwrite_path);
        if !source_path.exists() {
            continue;
        }

        let dest_path = game_dir.join(overwrite_path);
        let game_dir = game_dir.to_owned();
        copy_tasks.push(tokio::spawn(async move {
            debug!(
                "Copying {} to {}",
                source_path.display(),
                dest_path.display()
            );
            if source_path.is_dir() {
                fs_extra::dir::copy(
                    source_path,
                    game_dir,
                    &fs_extra::dir::CopyOptions::new()
                        .overwrite(true)
                        .copy_inside(true),
                )
                .context("Could not copy BepInEx files")
            } else {
                fs::copy(source_path, dest_path).context("Could not copy BepInEx files")
            }
        }));
    }

    for task in copy_tasks {
        dbg!(task.await.unwrap())?;
    }

    Ok(())
}

async fn install_unitas(game_dir: &Path, unitas_version: DownloadVersion) -> Result<()> {
    // overwrite install without overwriting the config and other important files
    let unitas_dir = paths::unitas_dir()?;
    let unitas_dir = match unitas_version {
        DownloadVersion::Stable => unitas_dir.join(paths::STABLE_DIR_NAME),
        DownloadVersion::Tag(tag) => unitas_dir.join(paths::TAG_DIR_NAME).join(tag),
        DownloadVersion::Branch(branch) => unitas_dir.join(paths::BRANCH_DIR_NAME).join(branch),
    };

    tokio::fs::copy(unitas_dir, game_dir).await?;

    Ok(())
}

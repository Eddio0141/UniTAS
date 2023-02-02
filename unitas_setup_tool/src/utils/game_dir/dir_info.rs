#[cfg(target_family = "unix")]
use std::os::unix::prelude::PermissionsExt;
use std::{
    io,
    path::{Path, PathBuf},
};

use semver::Version;

pub struct DirInfo {
    game_platform: GamePlatform,
    installed_info: Option<InstalledInfo>,
}

impl DirInfo {
    pub fn from_dir(game_dir: &Path) -> Result<Self, super::error::Error> {
        let game_platform = DirInfo::game_platform(game_dir)?;
        let installed_info = match InstalledInfo::from_dir(game_dir) {
            Ok(info) => Some(info),
            Err(super::error::Error::NotUnityGameDir) => None,
            Err(err) => return Err(err),
        };

        Ok(DirInfo {
            game_platform,
            installed_info,
        })
    }

    fn game_platform(game_dir: &Path) -> Result<GamePlatform, super::error::Error> {
        todo!()
    }
}

pub enum GamePlatform {
    Windows,
    Unix,
}

pub struct InstalledInfo {
    unitas_version: Option<InstalledVersion>,
    bepinex_version: Option<InstalledVersion>,
}

impl InstalledInfo {
    pub fn from_dir(game_dir: &Path) -> Result<Self, super::error::Error> {
        if !is_unity_dir(game_dir)? {
            return Err(super::error::Error::NotUnityGameDir);
        }

        let bepinex_version = InstalledInfo::bepinex_version(game_dir)?;
        let unitas_version = InstalledInfo::unitas_version(game_dir)?;

        Ok(InstalledInfo {
            bepinex_version,
            unitas_version,
        })
    }

    fn bepinex_version(game_dir: &Path) -> Result<Option<InstalledVersion>, super::error::Error> {
        todo!()
    }

    fn unitas_version(game_dir: &Path) -> Result<Option<InstalledVersion>, super::error::Error> {
        todo!()
    }
}

pub enum InstalledVersion {
    Branch(String),
    Semver(Version),
}

/// Returns true if the directory is a Unity game directory.
/// - Checks {dir}/{game_name}_Data/Managed/UnityEngine.dll or UnityEngine.CoreModule.dll
fn is_unity_dir(dir: &Path) -> Result<bool, io::Error> {
    let all_exe = all_exe_in_dir(dir)?
        .iter()
        .filter_map(|exe| {
            exe.file_name().map(|name| {
                let name = name.to_string_lossy().to_string();
                // remove .exe
                name.strip_suffix(".exe")
                    .map(|name| name.to_string())
                    .unwrap_or(name)
            })
        })
        .collect::<Vec<_>>();

    if all_exe.is_empty() {
        return Ok(false);
    }

    let all_data_dirs = std::fs::read_dir(dir)?;

    for data_dir in all_data_dirs {
        let data_dir = data_dir?;
        let data_dir_name = data_dir.file_name();
        let data_dir_name = data_dir_name.as_os_str().to_string_lossy();

        // match with exe name
        let Some(data_dir_name) = data_dir_name.strip_suffix("_Data") else{
            continue;
        };
        let data_dir_name = data_dir_name.to_string();
        if !all_exe.contains(&data_dir_name) {
            continue;
        }

        let managed_dir = data_dir.path().join("Managed");

        if !managed_dir.exists() {
            continue;
        }

        // check if UnityEngine.dll or UnityEngine.CoreModule.dll exists
        let unity_engine_dll = managed_dir.join("UnityEngine.dll");
        let unity_engine_core_module_dll = managed_dir.join("UnityEngine.CoreModule.dll");

        if unity_engine_dll.exists() || unity_engine_core_module_dll.exists() {
            return Ok(true);
        }
    }

    Ok(false)
}

fn all_exe_in_dir(dir: &Path) -> Result<Vec<PathBuf>, io::Error> {
    let mut all_exe = Vec::new();

    let all_files = std::fs::read_dir(dir)?;

    for file in all_files {
        let file = file?;
        if file.path().is_dir() {
            continue;
        }

        let file_name = file.file_name();
        let file_name = file_name.to_string_lossy();

        if file_name.ends_with(".exe") {
            all_exe.push(file.path());
            continue;
        }

        // unix only
        #[cfg(target_family = "unix")]
        {
            let permissions = file.metadata()?.permissions().mode();
            if permissions & 0o111 != 0 {
                all_exe.push(file.path());
            }
        }
    }

    Ok(all_exe)
}

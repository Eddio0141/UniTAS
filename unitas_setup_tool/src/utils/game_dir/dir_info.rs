#[cfg(target_family = "unix")]
use std::os::unix::prelude::PermissionsExt;
use std::{
    fmt::Display,
    io,
    path::{Path, PathBuf},
};

use anyhow::anyhow;

use crate::{
    prelude::Wrap,
    utils::{assembly_version::AssemblyVersion, exe_info::FileBitness, paths},
};

#[derive(Default)]
pub struct DirInfo {
    pub is_unity_dir: bool,
    pub game_platform: GamePlatform,
    pub installed_info: InstalledInfo,
}

impl DirInfo {
    pub fn from_dir(game_dir: &Path) -> Result<Self, super::error::Error> {
        let is_unity_dir = is_unity_dir(game_dir)?;

        if !is_unity_dir {
            return Ok(Default::default());
        }

        let game_platform = DirInfo::game_platform(game_dir)?;
        let installed_info = match InstalledInfo::from_dir(game_dir) {
            Ok(info) => info,
            Err(super::error::Error::NotUnityGameDir) => {
                unreachable!("is_unity_dir() should have returned false")
            }
            Err(err) => return Err(err),
        };

        Ok(DirInfo {
            is_unity_dir,
            game_platform,
            installed_info,
        })
    }

    fn game_platform(game_dir: &Path) -> Result<GamePlatform, super::error::Error> {
        // if the game dir contains a .exe file, it's probably a Windows game
        // otherwise, if the current platform is Unix based, it's probably a Unix game

        let files = game_dir.read_dir()?;

        for file in files {
            let file = file?;
            let file_name = file.file_name();
            let file_name = file_name.to_string_lossy();

            if file_name.ends_with(".exe") {
                let bitness = FileBitness::from_exe(&file.path())?;
                let platform = match bitness {
                    FileBitness::X86 => GamePlatform::Windowsx86,
                    FileBitness::X64 => GamePlatform::Windowsx64,
                    FileBitness::Other => {
                        return Err(super::error::Error::Other(anyhow!(
                            "File is not a windows executable, despite having .exe extension"
                        )));
                    }
                };
                return Ok(platform);
            }
        }

        let default_case = if cfg!(target_family = "unix") {
            GamePlatform::Unix
        } else {
            GamePlatform::Unknown
        };

        Ok(default_case)
    }
}

impl Display for DirInfo {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let Self {
            is_unity_dir,
            game_platform,
            installed_info,
        } = self;

        if !is_unity_dir {
            writeln!(f, "Not a Unity game directory")?;
            return Ok(());
        }

        writeln!(f, "Game Platform: {game_platform}")?;
        let default = "Not Installed".to_string();

        writeln!(
            f,
            "UniTAS Version: {}",
            installed_info
                .unitas_version
                .as_ref()
                .map(|version| version.to_string())
                .unwrap_or_else(|| default.clone())
        )?;
        writeln!(
            f,
            "BepInEx Version: {}",
            installed_info
                .bepinex_version
                .as_ref()
                .map(|version| version.to_string())
                .unwrap_or(default)
        )?;

        Ok(())
    }
}

#[derive(Default)]
pub enum GamePlatform {
    #[default]
    Unknown,
    Windowsx86,
    Windowsx64,
    Unix,
}

impl Display for GamePlatform {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            GamePlatform::Unknown => write!(f, "Unknown"),
            GamePlatform::Windowsx86 => write!(f, "Windows x86"),
            GamePlatform::Windowsx64 => write!(f, "Windows x64"),
            GamePlatform::Unix => write!(f, "Unix"),
        }
    }
}

#[derive(Default)]
pub struct InstalledInfo {
    unitas_version: Option<AssemblyVersion>,
    bepinex_version: Option<AssemblyVersion>,
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

    fn bepinex_version(game_dir: &Path) -> Result<Option<AssemblyVersion>, super::error::Error> {
        // checks in BepInEx/core/BepInEx.dll
        let bepinex_dir = game_dir.join("BepInEx");
        let bepinex_core_dir = bepinex_dir.join("core");
        let bepinex_dll = bepinex_core_dir.join("BepInEx.dll");

        if !bepinex_dll.try_exists()? {
            return Ok(None);
        }

        let version_info = AssemblyVersion::try_from(Wrap(bepinex_dll.as_path()))?;

        Ok(Some(version_info))
    }

    fn unitas_version(game_dir: &Path) -> Result<Option<AssemblyVersion>, super::error::Error> {
        // checks in BepInEx/plugins/UniTASPlugin.dll
        let bepinex_dir = game_dir.join("BepInEx");
        let unitas_dll = bepinex_dir
            .join(paths::unitas_plugins_dir())
            .join("UniTASPlugin.dll");

        if !unitas_dll.try_exists()? {
            return Ok(None);
        }

        let version_info = AssemblyVersion::try_from(Wrap(unitas_dll.as_path()))?;

        Ok(Some(version_info))
    }
}

/// Returns true if the directory is a Unity game directory.
/// - Checks {dir}/{game_name}_Data/Managed/UnityEngine.dll or UnityEngine.CoreModule.dll
fn is_unity_dir(dir: &Path) -> Result<bool, io::Error> {
    let all_exe = all_exe_in_dir(dir)?
        .iter()
        .filter_map(|exe| {
            let exe_extension = exe.extension().map(|ext| ext.to_string_lossy());
            let exe_extension = exe_extension.map(|exe_extension| format!(".{exe_extension}"));

            exe.file_name().map(|name| {
                let name = name.to_string_lossy().to_string();

                // remove extension
                match exe_extension {
                    Some(exe_extension) => name
                        .strip_suffix(&exe_extension)
                        .map(|name| name.to_string())
                        .unwrap_or(name),
                    None => name,
                }
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
        let Some(data_dir_name) = data_dir_name.strip_suffix("_Data") else {
            continue;
        };
        let data_dir_name = data_dir_name.to_string();
        if !all_exe.contains(&data_dir_name) {
            continue;
        }

        let managed_dir = data_dir.path().join("Managed");

        if !managed_dir.try_exists()? {
            continue;
        }

        // check if UnityEngine.dll or UnityEngine.CoreModule.dll exists
        let unity_engine_dll = managed_dir.join("UnityEngine.dll");
        let unity_engine_core_module_dll = managed_dir.join("UnityEngine.CoreModule.dll");

        if unity_engine_dll.try_exists()? || unity_engine_core_module_dll.try_exists()? {
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

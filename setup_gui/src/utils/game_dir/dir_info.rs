use std::path::Path;

use semver::Version;

pub struct DirInfo {
    game_platform: GamePlatform,
    installed_info: InstalledInfo,
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
        let unitas_version = super::unitas::get_installed_version(game_dir)?;
        let bepinex_version = super::bepinex::get_installed_version(game_dir)?;

        Ok(Self {
            unitas_version,
            bepinex_version,
        })
    }

    fn unitas_version(game_dir: &Path) -> Result<Option<InstalledVersion>, super::error::Error> {
        let unitas_version = super::unitas::get_installed_version(game_dir)?;

        Ok(unitas_version)
    }
}

pub enum InstalledVersion {
    Unknown,
    Semver(Version),
}

fn is_unity_dir(dir: &Path) -> bool {
}

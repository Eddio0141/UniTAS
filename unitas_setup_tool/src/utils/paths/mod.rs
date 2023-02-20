//! Paths to various directories and files.

pub mod error;

use std::path::{Path, PathBuf};

pub fn create_dir_if_not_exists(path: &Path) -> Result<(), self::error::Error> {
    if !path.is_dir() {
        std::fs::create_dir_all(path)?;
    }

    Ok(())
}

fn data_storage_dir() -> Result<PathBuf, self::error::Error> {
    let mut path = dirs::data_local_dir().ok_or(self::error::Error::NoLocalDataDir)?;
    path.push(env!("CARGO_PKG_NAME"));
    Ok(path)
}

pub fn local_bepinex_dir() -> Result<PathBuf, self::error::Error> {
    let mut path = data_storage_dir()?;
    path.push("BepInEx");
    create_dir_if_not_exists(&path)?;
    Ok(path)
}

pub fn local_unitas_dir() -> Result<PathBuf, self::error::Error> {
    let mut path = data_storage_dir()?;
    path.push("UniTAS");
    create_dir_if_not_exists(&path)?;
    Ok(path)
}

pub fn history_path() -> Result<PathBuf, self::error::Error> {
    let mut path = data_storage_dir()?;
    path.push("history.json");
    Ok(path)
}

/// A path to the directory where UniTAS plugins are in an artifact.
pub fn unitas_plugins_dir() -> PathBuf {
    Path::new("plugins").join("UniTAS")
}

pub const TAG_DIR_NAME: &str = "tag";
pub const BRANCH_DIR_NAME: &str = "branch";
pub const STABLE_DIR_NAME: &str = "stable";

pub fn local_tag_path(path: &Path) -> PathBuf {
    path.join(TAG_DIR_NAME)
}

pub fn local_branch_path(path: &Path) -> PathBuf {
    path.join(BRANCH_DIR_NAME)
}

pub fn local_stable_path(path: &Path) -> PathBuf {
    path.join(STABLE_DIR_NAME)
}

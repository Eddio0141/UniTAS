pub mod error;

use std::path::{Path, PathBuf};

fn create_dir_if_not_exists(path: &Path) -> Result<(), self::error::Error> {
    if !path.is_dir() {
        std::fs::create_dir_all(path)?;
    }

    Ok(())
}

fn data_storage_dir() -> Result<PathBuf, self::error::Error> {
    let mut path = dirs::data_local_dir().ok_or(self::error::Error::NoLocalDataDir)?;
    path.push("UniTAS");
    Ok(path)
}

pub fn bepinex_dir() -> Result<PathBuf, self::error::Error> {
    let mut path = data_storage_dir()?;
    path.push("BepInEx");
    create_dir_if_not_exists(&path)?;
    Ok(path)
}

pub fn unitas_dir() -> Result<PathBuf, self::error::Error> {
    let mut path = data_storage_dir()?;
    path.push("UniTAS");
    create_dir_if_not_exists(&path)?;
    Ok(path)
}

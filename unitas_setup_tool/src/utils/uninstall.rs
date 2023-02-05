use std::path::{Path, PathBuf};

use anyhow::{Context, Result};
use log::debug;

use super::cli::GameDirSelection;

pub async fn uninstall(game_dir_selection: &GameDirSelection) -> Result<()> {
    let dir = PathBuf::try_from(game_dir_selection.clone())?;

    let remove_paths = vec![Path::new("BepInEx/plugins/UniTASPlugin.dll")];

    for remove_path in remove_paths {
        let path = dir.join(remove_path);
        debug!("Removing {}", path.display());
        if path.is_file() {
            std::fs::remove_file(&path)
                .with_context(|| format!("Failed to remove file at {}", path.display()))?;
        } else if path.is_dir() {
            std::fs::remove_dir_all(&path)
                .with_context(|| format!("Failed to remove directory at {}", path.display()))?;
        } else {
            unreachable!("Path is neither file nor directory: {}", path.display());
        }
    }

    Ok(())
}

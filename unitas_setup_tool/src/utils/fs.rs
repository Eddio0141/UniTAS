use std::{fs, path::Path};

use anyhow::{bail, Context, Result};

/// Copies a directory recursively.
pub fn copy_dir_all(from: &Path, to: &Path, overwrite: bool) -> Result<()> {
    if !from.is_dir() {
        bail!("{} is not a directory", from.display());
    }

    if !to.is_dir() {
        fs::create_dir_all(to)
            .with_context(|| format!("Failed to create directory {}", to.display()))?;
    }

    let mut found_from = Vec::new();
    let mut found_to = Vec::new();
    let mut found_dirs = vec![from.to_path_buf()];

    while let Some(dir) = found_dirs.pop() {
        let entries = dir
            .read_dir()
            .with_context(|| format!("Failed to read directory {}", dir.display()))?;
        for entry in entries {
            let entry =
                entry.with_context(|| format!("Failed to read directory {}", dir.display()))?;
            let path = entry.path();

            if path.is_dir() {
                found_dirs.push(path);
            } else {
                let Some(from) = path.file_name() else {
                    bail!("Failed to get file name from {}", path.display());
                };

                let to = to.join(from);
                found_from.push(path);
                found_to.push(to);
            }
        }
    }

    for (from, to) in found_from.iter().zip(found_to.iter()) {
        if to.exists() && !overwrite {
            continue;
        }

        std::fs::copy(from, to)?;
    }

    Ok(())
}

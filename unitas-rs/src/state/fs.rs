use std::{
    borrow::Cow,
    collections::HashSet,
    path::{Path, PathBuf},
    sync::{LazyLock, Mutex},
};

use log::error;

use crate::utils::{copy_dir_all, unique_temp_dir};

pub static FS: LazyLock<Mutex<Fs>> = LazyLock::new(Mutex::default);

#[derive(Default)]
pub struct Fs {
    /// Extracted file system for movie
    ///
    /// - This will make fs operations redirect here unless passthrough is specified
    movie_fs: Option<PathBuf>,
    /// Passthrough paths
    ///
    /// - Always takes priority and forwards fs operations to host system always
    passthrough: HashSet<PathBuf>,
}

impl Fs {
    pub fn movie_start(&mut self, fs_path: Option<&Path>, passthrough: &[&Path]) {
        let fs = match fs_path {
            Some(movie_path) => {
                let fs = unique_temp_dir().join("filesystem");
                if let Err(err) = copy_dir_all(movie_path, &fs) {
                    error!(
                        "cannot setup virtual file system, failed to copy file system from `{}` to `{}`: {err:?}",
                        movie_path.display(),
                        fs.display()
                    );
                    return;
                }

                Some(fs)
            }
            None => None,
        };

        self.movie_fs = fs;
        self.passthrough.clear();
        for passthrough in passthrough {
            if self.passthrough.contains(*passthrough) {
                continue;
            }
            self.passthrough.insert(passthrough.to_path_buf());
        }
    }

    pub fn movie_end(&mut self) {
        self.passthrough.clear();
        self.movie_fs = None;
    }

    pub fn handle_path<'a>(&self, path: &'a Path) -> Cow<'a, Path> {
        let Some(movie_fs) = &self.movie_fs else {
            return Cow::Borrowed(path);
        };

        if self.passthrough.contains(path) {
            return Cow::Borrowed(path);
        }

        Cow::Owned(movie_fs.join(path))
    }
}

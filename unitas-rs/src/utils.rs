use std::{
    env::temp_dir,
    ffi::{CStr, c_char},
    fs::{self, File},
    io,
    path::{Path, PathBuf},
};

use log::error;
use rand::{RngExt, distr::Alphanumeric};
use zip::ZipArchive;

pub fn unique_temp_dir() -> PathBuf {
    let tmp = temp_dir();

    loop {
        let dir_name = rand::rng()
            .sample_iter(&Alphanumeric)
            .take(10)
            .map(char::from)
            .collect::<String>();

        let path = tmp.join(dir_name);
        if !path.is_dir() {
            return path;
        }
    }
}

pub fn copy_dir_all(src: impl AsRef<Path>, dst: impl AsRef<Path>) -> io::Result<()> {
    fs::create_dir_all(&dst)?;

    for entry in fs::read_dir(src)? {
        let entry = entry?;
        if entry.file_type()?.is_dir() {
            copy_dir_all(entry.path(), dst.as_ref().join(entry.file_name()))?;
        } else {
            fs::copy(entry.path(), dst.as_ref().join(entry.file_name()))?;
        }
    }

    Ok(())
}

#[unsafe(no_mangle)]
pub extern "C" fn extract_zip(target: *const c_char, dest: *const c_char) -> bool {
    let target = unsafe { CStr::from_ptr(target) };
    let dest = unsafe { CStr::from_ptr(dest) };
    let target = Path::new(target.to_str().unwrap());
    let dest = Path::new(dest.to_str().unwrap());

    let fs_zip_file = match File::open(target) {
        Ok(value) => value,
        Err(err) => {
            error!("file extraction error: {err:?}",);
            return false;
        }
    };

    let mut fs_archive = match ZipArchive::new(fs_zip_file) {
        Ok(value) => value,
        Err(err) => {
            error!("file extraction error: {err:?}",);
            return false;
        }
    };

    if let Err(err) = fs_archive.extract(dest) {
        error!("file extraction error: {err:?}",);
        return false;
    }

    true
}

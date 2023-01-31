pub mod error;

use std::{
    fs::File,
    io::BufReader,
    path::{Path, PathBuf},
};

use serde::{Deserialize, Serialize};

use super::paths;

#[derive(Serialize, Deserialize)]
pub struct History {
    entries: Vec<PathBuf>,
}

impl History {
    pub fn load() -> Result<Self, self::error::Error> {
        let path = paths::history_path()?;
        let file = File::open(path)?;
        let reader = BufReader::new(file);
        let history = serde_json::from_reader(reader)?;

        Ok(history)
    }

    pub fn save(&self) -> Result<(), self::error::Error> {
        let path = paths::history_path()?;
        let file = File::create(path)?;
        serde_json::to_writer_pretty(file, self)?;

        Ok(())
    }

    pub fn index(&self, index: usize) -> Result<&Path, self::error::Error> {
        self.entries
            .get(index)
            .map(|path| path.as_path())
            .ok_or_else(|| self::error::Error::HistoryIndexOutOfRange {
                index,
                history_size: self.entries.len(),
            })
    }

    pub fn add(&mut self, path: PathBuf) {
        self.entries.insert(0, path);
    }
}

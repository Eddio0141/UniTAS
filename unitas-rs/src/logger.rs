use std::{
    env,
    fs::File,
    io::{self, BufWriter, Write},
    sync::{Arc, Mutex},
    thread,
    time::{Duration, SystemTime},
};

use colored::Colorize;
use time::{OffsetDateTime, format_description};

use crate::hook::hooks::ReverseInvoke;

pub struct DiskLogger {
    output_file: Arc<Mutex<BufWriter<File>>>,
}

impl DiskLogger {
    pub fn new() -> io::Result<Self> {
        // as of now, the BepInEx directory is guaranteed to exist so create_buffered will not fail
        let output_file = env::current_exe()
            .expect("failed to get current exe")
            .parent()
            .unwrap()
            .join("BepInEx")
            .join("unitas-rs.log");
        let output_file = Arc::new(Mutex::new(BufWriter::new(File::create(output_file)?)));
        let output_file_ = Arc::clone(&output_file);

        // TODO: as of now, I can't flush in case of crashes
        thread::spawn(move || {
            loop {
                thread::sleep(Duration::from_secs(2));
                let ri = ReverseInvoke::new();
                output_file_.lock().unwrap().flush().unwrap();
                drop(ri);
            }
        });

        Ok(DiskLogger { output_file })
    }
}

impl log::Log for DiskLogger {
    fn enabled(&self, _metadata: &log::Metadata) -> bool {
        true
    }

    fn log(&self, record: &log::Record) {
        let ri = ReverseInvoke::new();

        let time = {
            let time_format =
                format_description::parse("[year]-[month]-[day] [hour]:[minute]:[second]");
            match time_format {
                Ok(time_format) => {
                    let time = OffsetDateTime::from(SystemTime::now()).format(&time_format);
                    match time {
                        Ok(time) => time,
                        Err(err) => format!("unknown_time: {err:?}"),
                    }
                }
                Err(err) => format!("unknown_time: {err:?}"),
            }
        };

        let level = record.level();
        let level = match level {
            log::Level::Error => level.as_str().red(),
            log::Level::Warn => level.as_str().yellow(),
            log::Level::Info => level.as_str().green(),
            log::Level::Debug => level.as_str().blue(),
            log::Level::Trace => level.as_str().cyan(),
        };
        println!(
            "{}{time} {} {}{} {}",
            "[".truecolor(76, 76, 76),
            level,
            record.target(),
            "]".truecolor(76, 76, 76),
            record.args()
        );
        {
            let mut output_file = self.output_file.lock().unwrap();
            let format = format!(
                "[{time} {} {}] {}\n",
                record.level().as_str(),
                record.target(),
                record.args()
            );
            if let Err(err) = output_file.write_all(format.as_bytes()) {
                println!("failed to write disk log: {err:?}");
            }
        }

        drop(ri);
    }

    fn flush(&self) {
        let ri = ReverseInvoke::new();
        self.output_file.lock().unwrap().flush().unwrap();
        drop(ri);
    }
}

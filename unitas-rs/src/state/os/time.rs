/*
use std::{
    sync::{LazyLock, Mutex},
    time::{Duration, SystemTime},
};

use log::debug;

use crate::{
    error::{Error, Result},
    hook::hooks::ReverseInvoke,
};

const YEAR_SECS: u64 = 31556952;
const UNIX_EPOCH_DURATION: Duration = Duration::from_secs(1970 * YEAR_SECS);

pub static TIME: LazyLock<Mutex<Time>> = LazyLock::new(|| {
    let ri = ReverseInvoke::new();
    let time = SystemTime::now()
        .duration_since(SystemTime::UNIX_EPOCH)
        .unwrap()
        + UNIX_EPOCH_DURATION;
    drop(ri);
    Mutex::new(Time::new(time))
});

pub struct Time {
    initial_time: Duration,
    time: Duration,
    cpu_time: Duration,
}

impl Time {
    pub fn new(initial_time: Duration) -> Self {
        Time {
            initial_time,
            cpu_time: initial_time,
            time: initial_time,
        }
    }

    pub fn sleep(&mut self, duration: Duration) {
        self.time += duration;
        self.time_update += duration;
        self.time_fixed_update += duration;
    }

    pub fn add_frame_time(&mut self, add: Duration) {
        self.time += add;
        self.cpu_time += add;
        debug!("time: update, advance by {add:?}");
    }

    pub fn monotonic(&self) -> Duration {
        self.time - self.initial_time
    }

    pub fn monotonic_cpu(&self) -> Duration {
        self.cpu_time - self.initial_time
    }

    pub fn restart(&mut self, time: Duration) -> Result<()> {
        if time < UNIX_EPOCH_DURATION {
            return Err(Error::RestartTimeBeforeUnixEpoch);
        }

        self.time = time;
        self.cpu_time = time;
        Ok(())
    }
}
*/

use std::{
    collections::HashMap,
    hash::{Hash, Hasher},
    sync::{LazyLock, Mutex},
    time::{Duration, SystemTime},
};

use backtrace::Backtrace;
use log::{trace, warn};
use rustc_hash::FxHasher;

use crate::{hook::hooks::ReverseInvoke, info};

const YEAR_SECS: u64 = 31556952;
const UNIX_EPOCH: Duration = Duration::from_secs(1970 * YEAR_SECS);

pub static TIME: LazyLock<Mutex<Time>> = LazyLock::new(|| {
    let ri = ReverseInvoke::new();
    let time = SystemTime::now()
        .duration_since(SystemTime::UNIX_EPOCH)
        .unwrap()
        + UNIX_EPOCH;
    drop(ri);
    Mutex::new(Time::new(time))
});

pub struct Time {
    /// Time since startup of game, very specific to unity
    time_since_startup: Duration,
    /// System boot time
    initial_time: Duration,
    /// Actual time
    time: Duration,
    /// Fake time, used to try advance busy loops
    fake_time: Duration,
    /// Duration of 1 frame
    frame_time: Duration,
    busyloop: Busyloop,
}

impl Time {
    pub fn new(initial_time: Duration) -> Self {
        trace!("{initial_time:?}");
        Time {
            time_since_startup: Duration::ZERO,
            initial_time,
            time: initial_time,
            fake_time: Duration::from_secs(YEAR_SECS) * 1,
            frame_time: Duration::from_secs_f64(1. / 60.),
            busyloop: Busyloop::default(),
        }
    }

    pub fn set_frame_time(&mut self, frame_time: Duration) {
        self.frame_time = frame_time;
    }

    pub fn add_frame_time(&mut self) {
        self.time += self.frame_time;
        self.fake_time = self.time;
        self.time_since_startup = self.time;
        trace!("adding frametime, {:?}", self.time);

        self.busyloop.reset();
    }

    pub fn get(&mut self, time_type: Option<TimeType>) -> Duration {
        let monotonic = self.monotonic();

        if !info::is_main_thread() {
            return monotonic;
        }

        if self.busyloop.detect(time_type) {
            self.fake_time += Duration::from_nanos(1);
            trace!("advancing fake timer");
            return self.fake_time;
        }

        monotonic
    }

    pub fn time_since_startup(&mut self) -> Duration {
        if !info::is_main_thread() {
            return self.time_since_startup;
        }

        if self.busyloop.detect(None) {
            self.time_since_startup += Duration::from_millis(1);
            trace!("advancing fake timer");
        }

        self.time_since_startup
    }

    fn monotonic(&self) -> Duration {
        self.time - self.initial_time
    }

    pub fn restart(&mut self, mut time: Duration) {
        if time < UNIX_EPOCH {
            warn!("time is less than epoch time, setting time to epoch time");
            time = UNIX_EPOCH;
        }

        self.time_since_startup = Duration::ZERO;
        self.initial_time = time;
        self.time = time;
        self.fake_time = time;
        self.busyloop.reset();
    }
}

#[cfg(unix)]
pub type TimeType = libc::clockid_t;
#[cfg(windows)]
pub type TimeType = u8;

#[derive(Default)]
struct Busyloop {
    hash_counter: HashMap<u64, usize>,
}

const BUSYLOOP_LIMIT: usize = 20;

impl Busyloop {
    fn detect(&mut self, time_type: Option<TimeType>) -> bool {
        // same backtrace?
        let mut hash = FxHasher::default();

        time_type.hash(&mut hash);

        let bt = Backtrace::new_unresolved();
        for frame in bt.frames().iter().skip(1) {
            frame.ip().hash(&mut hash);
        }

        let hash = hash.finish();

        let counter = match self.hash_counter.get_mut(&hash) {
            Some(value) => value,
            None => {
                self.hash_counter.insert(hash, 0);
                self.hash_counter.get_mut(&hash).unwrap()
            }
        };

        *counter += 1;

        if *counter < BUSYLOOP_LIMIT {
            return false;
        }

        self.hash_counter.clear();

        true
    }

    fn reset(&mut self) {
        self.hash_counter.clear();
    }
}

#[cfg(test)]
mod tests {
    use crate::state::os::{Busyloop, TimeType};

    #[test]
    fn busyloop_single_call_single_type() {
        let mut busyloop = Busyloop::default();
        let mut was_busy = false;

        for _ in 0..200 {
            if busyloop.detect(None) {
                was_busy = true;
                break;
            }
        }

        assert!(was_busy);
    }

    #[test]
    fn busyloop_single_call_multi_type() {
        let mut busyloop = Busyloop::default();
        let mut was_busy = false;

        for _ in 0..200 {
            if busyloop.detect(None) || busyloop.detect(Some(1)) {
                was_busy = true;
                break;
            }
        }

        assert!(was_busy);
    }

    fn multi_call(busyloop: &mut Busyloop, time_type: Option<TimeType>) -> bool {
        busyloop.detect(time_type)
    }

    #[test]
    fn busyloop_multi_call_multi_type() {
        let mut busyloop = Busyloop::default();
        let mut was_busy = false;

        for _ in 0..200 {
            if multi_call(&mut busyloop, None) || multi_call(&mut busyloop, Some(1)) {
                was_busy = true;
                break;
            }
        }

        assert!(was_busy);
    }
}

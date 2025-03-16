use std::result;

use thiserror::Error;

pub type Result<T> = result::Result<T, Error>;

#[derive(Error, Debug)]
pub enum Error {
    #[error("attempt to set system time to before unix epoch: 1970-01-01 00:00:00 UTC")]
    RestartTimeBeforeUnixEpoch,
}

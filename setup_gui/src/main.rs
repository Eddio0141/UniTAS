//! Tool that sets up UniTAS for an unity game.
//!
//! # Features
//! - Get UniTAS and BepInEx version on game directory
//! - Install UniTAS on game directory
//!     - You can choose the version to install, which gets downloaded from GitHub
//!     - By default, the latest version is installed
//!     - By default, BepInEx 5.x is installed
//!     - You can also choose the version of BepInEx to install
//! - Uninstall UniTAS from game directory
//!     - You can fully uninstall UniTAS along with BepInEx or not
//! - History of visited game directories
//! - List locally availiable UniTAS versions
//! - List locally availiable BepInEx versions
//! - Offline mode

mod error;
mod prelude;
mod utils;

fn main() {}
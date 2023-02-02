pub mod error;

use std::thread;

use self::error::Error;

use super::{cli::VersionSelection, github_artifact::Action};

const UNITAS_OWNER: &str = "eddio0141";
const UNITAS_REPO: &str = "UniTAS";

pub async fn download_unitas(version: &VersionSelection) -> Result<(), Error> {
    match version {
        VersionSelection::Stable => todo!(),
        VersionSelection::Branch(branch) => {
            let action = Action::get_latest_action(UNITAS_OWNER, UNITAS_REPO, branch).await?;
            dbg!(action);
        }
        VersionSelection::SemVer(_) => todo!(),
    }

    Ok(())
}

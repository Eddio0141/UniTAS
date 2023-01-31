#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Not an unity game directory")]
    NotUnityGameDir,
}

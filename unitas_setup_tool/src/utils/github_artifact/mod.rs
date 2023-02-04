use std::{fs, io::Cursor, path::Path};

use zip::ZipArchive;

use crate::prelude::Wrap;

use self::error::Error;

pub mod error;

#[derive(Debug, Clone)]
pub struct Artifact {
    pub name: String,
    pub url: String,
}

impl Artifact {
    pub async fn extract_to_dir(&self, dest_path: &Path) -> Result<(), Error> {
        let response = reqwest::get(&self.url).await?;
        let bytes = response.bytes().await?;

        let dest_path = dest_path.join(&self.name);

        // create directory
        fs::create_dir_all(&dest_path)?;

        // unzip
        let mut archive = ZipArchive::new(Cursor::new(bytes))?;

        archive.extract(dest_path)?;

        Ok(())
    }
}

#[derive(Debug)]
pub struct Build {
    pub artifacts: Vec<Artifact>,
}

impl Build {
    pub async fn latest_action_build(
        owner: &str,
        repo: &str,
        branch: &str,
        workflow_file_name: &str,
    ) -> Result<Self, Error> {
        let url = format!(
            "https://api.github.com/repos/{owner}/{repo}/actions/runs?branch={branch}&event=push&status=success"
        );
        // github requires us to have User-Agent header
        let client = Wrap::<reqwest::Client>::github_api_client()?;

        let response = client.get(&url).send().await?;
        let text = response.text().await?;
        let json: serde_json::Value =
            serde_json::from_str(&text).map_err(|error| Error::JsonParseError {
                error,
                response: text,
            })?;
        let artifacts = json
            .get("workflow_runs")
            .ok_or(Error::CantFindWorkflowRuns)?
            .get(0)
            .ok_or(Error::CantFindWorkflowRuns)?
            .get("artifacts_url")
            .ok_or(Error::CantFindArtifactsUrl)?
            .as_str()
            .ok_or(Error::CantFindArtifactsUrl)?
            .to_string();
        let response = client.get(&artifacts).send().await?;
        let text = response.text().await?;
        let json: serde_json::Value =
            serde_json::from_str(&text).map_err(|error| Error::JsonParseError {
                error,
                response: text,
            })?;
        let artifacts = json
            .get("artifacts")
            .ok_or(Error::UnexpectedArtifactsJsonData)?
            .as_array()
            .ok_or(Error::UnexpectedArtifactsJsonData)?
            .iter()
            .map(|artifact| {
                let Some(name) = artifact.get("name") else {
                    return Err(Error::UnexpectedArtifactsJsonData);
                };

                let Some(name) = name.as_str() else {
                    return Err(Error::UnexpectedArtifactsJsonData);
                };

                let name = name.to_string();
                let branch = branch.replace('/', "%2F");

                // because github doesn't allow us to download artifacts directly, we use nightly.link
                let url = format!(
                    "https://nightly.link/{owner}/{repo}/workflows/{workflow_file_name}/{branch}/{name}.zip"
                );

                Ok(Artifact { name, url })
            })
            .collect::<Result<Vec<_>, Error>>()?;

        Ok(Self { artifacts })
    }

    pub async fn latest_stable_build(owner: &str, repo: &str) -> Result<Self, Error> {
        let url = format!("https://api.github.com/repos/{owner}/{repo}/releases/latest");

        // github requires us to have User-Agent header
        let client = Wrap::<reqwest::Client>::github_api_client()?;

        let response = client.get(&url).send().await?;
        let text = response.text().await?;
        let json: serde_json::Value =
            serde_json::from_str(&text).map_err(|error| Error::JsonParseError {
                error,
                response: text,
            })?;

        let artifacts = json
            .get("assets")
            .ok_or(Error::UnexpectedArtifactsJsonData)?
            .as_array()
            .ok_or(Error::UnexpectedArtifactsJsonData)?
            .iter()
            .map(|artifact| {
                let url = artifact
                    .get("browser_download_url")
                    .ok_or(Error::UnexpectedArtifactsJsonData)?
                    .as_str()
                    .ok_or(Error::UnexpectedArtifactsJsonData)?
                    .to_string();

                // because the name is all the same, we get a unique name from the url
                let name = url
                    .split('/')
                    .last()
                    .ok_or(Error::UnexpectedArtifactsJsonData)?;

                let name = name.strip_suffix(".zip").unwrap_or(name).to_string();

                Ok(Artifact { name, url })
            })
            .collect::<Result<Vec<_>, Error>>()?;

        Ok(Self { artifacts })
    }

    pub async fn release_by_tag(owner: &str, repo: &str, tag: &str) -> Result<Self, Error> {
        let url = format!("https://api.github.com/repos/{owner}/{repo}/releases/tags/{tag}");

        // github requires us to have User-Agent header
        let client = Wrap::<reqwest::Client>::github_api_client()?;

        let response = client.get(&url).send().await?;
        let text = response.text().await?;
        let json: serde_json::Value =
            serde_json::from_str(&text).map_err(|error| Error::JsonParseError {
                error,
                response: text,
            })?;

        let artifacts = json
            .get("assets")
            .ok_or(Error::UnexpectedArtifactsJsonData)?
            .as_array()
            .ok_or(Error::UnexpectedArtifactsJsonData)?
            .iter()
            .map(|artifact| {
                let url = artifact
                    .get("browser_download_url")
                    .ok_or(Error::UnexpectedArtifactsJsonData)?
                    .as_str()
                    .ok_or(Error::UnexpectedArtifactsJsonData)?
                    .to_string();

                // because the name is all the same, we get a unique name from the url
                let name = url
                    .split('/')
                    .last()
                    .ok_or(Error::UnexpectedArtifactsJsonData)?;

                let name = name.strip_suffix(".zip").unwrap_or(name).to_string();

                Ok(Artifact { name, url })
            })
            .collect::<Result<Vec<_>, Error>>()?;

        Ok(Self { artifacts })
    }

    pub async fn extract_to_dir(&self, dest_path: &Path) -> Result<(), Error> {
        // spawn tasks
        let mut tasks = Vec::new();
        for artifact in &self.artifacts {
            let dest_path = dest_path.to_path_buf();
            let artifact = artifact.clone();
            let task = tokio::spawn(async move { artifact.extract_to_dir(&dest_path).await });
            tasks.push(task);
        }

        // wait for tasks
        for task in tasks {
            task.await??;
        }

        Ok(())
    }
}

impl Wrap<reqwest::Client> {
    pub fn github_api_client() -> Result<reqwest::Client, reqwest::Error> {
        // github requires us to have User-Agent header
        reqwest::Client::builder()
            .user_agent(env!("CARGO_PKG_NAME"))
            .build()
    }
}

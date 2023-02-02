use self::error::Error;

pub mod error;

#[derive(Debug)]
pub struct Artifact {
    pub name: String,
    pub url: String,
}

#[derive(Debug)]
pub struct Action {
    pub artifacts: Vec<Artifact>,
}

impl Action {
    pub async fn get_latest_action(owner: &str, repo: &str, branch: &str) -> Result<Self, Error> {
        let url = format!(
            "https://api.github.com/repos/{owner}/{repo}/actions/runs?branch={branch}&event=push&status=success"
        );
        // github requires us to have User-Agent header
        let client = reqwest::Client::builder()
            .user_agent(env!("CARGO_PKG_NAME"))
            .build()?;

        let response = client.get(&url).send().await?;
        let text = response.text().await?;
        let json: serde_json::Value =
            serde_json::from_str(&text).map_err(|error| Error::JsonParseError {
                error,
                response: text,
            })?;
        // TODO get index by get
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
        let artifacts = json.get("artifacts").ok_or(Error::UnexpectedArtifactsJsonData)?
            .as_array()
            .ok_or(Error::UnexpectedArtifactsJsonData)?
            .iter()
            .map(|artifact| {
                let (Some(name), Some(url)) = (artifact.get("name"), artifact.get("archive_download_url")) else {
                    return Err(Error::UnexpectedArtifactsJsonData);
                };

                let (Some(name), Some(url)) = (name.as_str(), url.as_str()) else {
                    return Err(Error::UnexpectedArtifactsJsonData);
                };

                let (name, url) = (name.to_string(), url.to_string());

                Ok(Artifact { name, url })
            })
            .collect::<Result<Vec<_>, Error>>()?;

        Ok(Self { artifacts })
    }
}

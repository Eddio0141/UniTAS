# Problem
Most modern unity versions have a method to tell what index is a scene from name, but in some old unity versions, there is no way to tell this

# Solution idea 1
- At the start of plugin load, load every single scene via `Application.levelCount`
- On each scene load, we use `Application.loadedLevel` and `Application.loadedLevelName` to tell what scene is what name and index
- Store this information in some helper tracker named something like `ISceneIndexNameTracker` and have another helper interface called `ISceneIndexName` to actually get those name / index
  - Use the internal-docs/reload-game-on-plugin-load.md's `IOnInitialLoad` interface for doing that operation at start and add the interface that invokes `Update` to tell what scene just loaded

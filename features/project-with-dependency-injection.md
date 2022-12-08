In order to make the project clean, we use a container and loosen coupled classes using dependency injection

# Overall structure
By default, TAS plugin features such as movie playback will be isolated using dependency injection

## Patches
Because patches don't get instantiated, we can't use dependencies on it, so we have to directly use `Plugin` instance to reference to the container, and get resources from there
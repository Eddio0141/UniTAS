# Problem
- On initial restart, I destroy all objects because I can't tell which one is DontDestroyOnLoad

# Solution
## Using patcher
This could be the easiest solution, where I modify DontDestroyOnLoad to track game objects that are marked DontDestroyOnLoad

### Implementation
- Patcher patches DontDestroyOnLoad, where upon invoked, it will store the object if its not null, is a game object type, and is the root of the game object
- The tracking list will be stored in `UniTAS.Patcher` namespace in a class `Tracker` called `DontDestroyOnLoad` as a property
  - Getter will check and remove all game objects if they are still valid
- `UniTAS.Plugin` will reference the `Tracker` and use the property as needed
# Why it should be implemented
Currently, the plugin is written in a way that wouldn't matter at what timing it loads in after the game loads.  

This worked fine with few codes that adapt to loading at any timing, however some tracking is very difficult to do when this is the case such as tracking `DontDestroyOnLoad` calls, which then I have to use a hacky solution such as finding all `UnityEngine.Object` instances and manually `Destroy` call them.  

Implementing this will make some tracking that depends on method calls easy to do and cleaner to do as the plugin will force the game to be restarted in a way.

# Implementation idea
- After the plugin loads, we do the usual patching, and other initial operations that's required
- We reload scene 0, destroy all game objects, and reset static fields. Basically like soft restarting the game
- We supress some other plugin functionalities until the initial reload happens, then everything else works
  - For this, we could create a separate `Container` from StructureMap, which only has the configuration to do the initial operations
  - I could also introduce an interface that works with the container, called something like `IOnInitialLoad` and method `void Load()`

# Implementation
- Initial plugin load
  - Plugin will first patch everything required
  - We use a helper that will store all static fields and also let us reset them
  - Stop `MonoBehavior` updates
  - Destroy all `UnityEngine.Object` instances
  - Let the initial game reloader do it's job
    - Similar to `GameRestart` but doesn't do everything
    - Make sure it syncs the update cycle
- Second plugin load
  - Load the main `Container`, now plugin will load as normal

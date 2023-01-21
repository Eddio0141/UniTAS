# Problem
Because unity doesn't have API to expose what objects are in the scene easily, so it's hard to know how many objects are there without hogging the performance with `FindObjectsOfType` invokes many times, or do a search easily

# Solution ideas for each scenarios

## On scene load
### Non additional scene load object destruction
- We first need to patch all types deriving from `UnityEngine.Object`, then patch the method `Finalize`
  - This is the main thing to tell if the instance is unloaded
- We also patch the scene load methods, where if the scene just got loaded, we can execute our method to tell if the objects has been destroyed
  - Important thing to make this work is we manually invoke `GC.Collect` and `GC.WaitForPendingFinilizers` right after the scene got loaded to force the invoke of `Finalize`
### New objects
- We first patch all types deriving from `UnityEngine.Object`, and patch the constructors for all
  - This is how we tell if the instance was loaded

## On scene unload
- Similar to scene load, we force invoke the GC and tell if the object got destroyed on unload

## On `Destroy` and `DestroyImmediate`
- These are simply checking the instance from the argument and updating tracking status

## On `Instantiate`
- This is simply also doing the same thing as scene load, we patch the constructor of the types that derives from `UnityEngine.Object` and that's how we tell

# Things to find out
- [ ] What happens if an object has children objects and we do `Destroy` on the parent? Does it also invoke `Destroy` or would we not know?

# Extra notes
- Make sure the `UnityEngine.Object` is assignable to the types for discovery, as `MonoBehavior` also is deriving from `UnityEngine.Object`

# Patching idea
- Patches have a purpose, which I will use a `Module` to contain this
- Each `Module` will patch some methods, which *may or may not exist*, depending on unity version or mscorelib
- Depending on the **detected version**, the appropriate patch version will be selected
  - If the version matches the range or directly matches the patch version, that is selected
  - If no version is matching in the ranges, then the closest lower match will be chosen
  - If no lower match is found then the closest version is used

# Implementation
- There is a class that patch types can derive from called `PatchType`
  - Derive into a patch type, such as `UnityPatch` that modules can use as an attribute, choosing what kind of patch we are using for later processing
- All classes using `PatchGroup` defined inside a **module** will be processed later, ill just call it patch group or something
- Each class instances using `PatchGroup` will contain multiple harmony patches, which will be used when the patch group is selected
- Patch group needs to be inherited into a more specific one that lets you choose what it's targetting

## Processing
- Use `PatchProcessor` to process a certain patch type like `UnityPatch`, then rest you have to implement in the processor classes

# Implementation structure
- Assuming already made a patch module such as `UnityPatch`, apply attribute onto a module class
- In the class, define all patch groups

## Base attribute for patch group
```cs
public abstract class PatchGroup : Attribute
{
}
```

## Module example
```cs
[UnityPatch]
public class InputPatchModule
{
    // Patch groups go in here
}
```

## Base processor
```cs
public abstract class PatchProcessor
{
    /// Patch type the processor is targetting
    protected abstract Type TargetPatchType();

    /// Method to determine if the patch group should be used
    /// Return an index on which group should be used
    /// Version is supplied for determining what should be used
    protected abstract int ChoosePatch(string version, IEnumarable<PatchGroup> patchGroups);

    public void ProcessModules()
    {
        // Logic goes here for actually processing and applying the patches
    }
}
```
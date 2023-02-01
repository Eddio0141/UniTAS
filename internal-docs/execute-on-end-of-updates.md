# Problem
- Executing some code after everything has finished updating is hard as you need to know every update calls
- It's easy if you want to detect this on something like `Update` as you can call this method on `LateUpdate`, but its hard if you want to execute something after all updates are finished, but want to still execute code before the next frame cycle starts

# Ideas
## Something with coroutines
- On unity, the last thing that gets ran on the script execution cycle is `yield WaitForEndOfFrame`
- We could register our own coroutine that loops infinitely, which would work until a new coroutine is registered and it gets executed after ours

### ~~Plan~~
- Create a virtual coroutine tracker, that knows what coroutines are running and what exists in them by keeping track of them
- Register our coroutine that invokes `yield WaitForEndOfFrame`
- Through the virtual coroutine tracker, invoke something when the last `yield WaitForEndOfFrame` is invoked
---
This is very hard or probably impossible, because how you can mix logic within the coroutine, you really can't analyze whats going on in there without actually executing the code

### ~~Plan 2~~ This doesn't work
- Register our coroutine that invokes `yield WaitForEndOfFrame`
- Need to test this, but if the order of coroutine executed is by registered order, then it might be possible to force load our coroutine to be last by re-registering when a new coroutine is invoked

## ~~Plan 3~~ Too tricky to get working
- Have a list of registered coroutines
- In unity, each coroutine gets executed through `UnityEngine.SetupCoroutine.InvokeMoveNext`, this method takes an `IEnumerator`, which we can use to check which coroutine is running
- After the coroutines are all done running, execute our code
- This is VERY hard to do
---
- [x] Check if the timing of the invoke is related with when the `yield` is supposed to be invoked
  - It does as expected and also as written on the unity execution order docs
- [x] Check if theres an alternative to `InvokeMoveNext` in early unity versions
  - There isn't, so instead we need to live patch `Class.<FooCoroutine>c__Iterator0.MoveNext()` when `StartCoroutine` is invoked
  - Careful not to patch something more than once as coroutine can be registered more than 1 time
- [x] Does timing of `MoveNext` in iterator match `UnityEngine.SetupCoroutine.InvokeMoveNext`
  - Yes

### Plan 4
- A coroutine can't be registered when `yield return new WaitForEndOfFrame()`
- There are multiple places where I have to consider
  - `OnDestroy` is the last update you can run anything before the next cycle
    - Track via `Destroy` calls and match by hash
  - `OnDisable` is the next
    - Track via component enabled status set to false
  - Default case I can handle by adding a coroutine via `OnGUI`
    - I first track all `MonoBehaviour` object instance
    - I have a list of which instance hasn't ran `OnGUI` yet
    - When the list is empty, I run the code
- Those updates don't matter
  - `OnApplicationPause` because you can't really change the scene from here
  - `OnApplicationQuit` since application is closing so no point
  - I don't need to direcly interact with `yield return new WaitForEndOfFrame()` because I handle it in an easier way

# Info
- The method `MonoBehaviour.StartCoroutine(string, object)` and `MonoBehaviour.StartCoroutine(string)` will always search for a method returning `IEnumerator`
- **Order of selection**
  - declared order
  - number of args acending (0 or 1)
- If choosing the method without the `object` overload, it will use the first method that matches name and return type, with no argument
- If choosing the method with `object` overload, it will use the first method that matches name, return type, and if the `object` Type can be assigned to the argument Type
  - This will be done in declared order, so if you pass a `float` and methods for arguments `int` and `float` is declared in order, it will choose `int` because its assignable
## Some tests
- If you register coroutine via `StartCoroutine`, and they yield return the same timing, does the order of the coroutine run as registered order
    - [x] Same script
      - It runs in order always
    - [x] Different script with execution order set
      - Runs in script exec order as expected
- [x] If you run a coroutine inside yield, does it run at the same timing
  - Not if you're registering after the `yield return`
  - If registered before `yield`, it gets prioritised for some reason to run before the coroutine you ran this in

# Implementation of plan 4
## Cycle of checks
- Before update cycle, reset `_processedOnGUI` and fill with `MonoBehaviour` objects that has `OnGUI` method
- On each tracked `MonoBehaviour` objects, when `OnGUI` is ran, remove entry from `_processedOnGUI`
- When `_processedOnGUI` && `_processedOnDestroy` && `_processedOnDisable` is empty, run end of update code

## New `MonoBehaviour` instance
- If the instance has `OnGUI`, keep hold of it in list `_monoBehWithOnGUI`
- Detecting this has to be done through checking in `Instantiate`, `AddComponent`, scene load -> `MonoBehaviour` ctor match
  - Another way to probably do this is adding the method `LateUpdate` if it doesn't exist, and if the `MonoBehaviour` class has `OnGUI` defined in it

## Detecting `MonoBehaviour` class destruction
- Using finilizer doesn't work, so I have to track `Destroy` methods
- On matching hash, remove from all tracking list

## New coroutine register
- Stop the old coroutine
- Register new coroutine on Postfix
- Make sure to not create an infinite loop by registering my own coroutine

## Script disable
- Does the class have the event function?
  - If so, add entry to list `_processedOnDisable`

## `MonoBehaviour` destruction
- Does the class have the event function?
  - If so, add entry to list `_processedOnDestroy`
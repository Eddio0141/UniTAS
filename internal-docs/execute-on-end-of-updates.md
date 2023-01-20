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

### Plan 3
- Have a list of registered coroutines
- In unity, each coroutine gets executed through `UnityEngine.SetupCoroutine.InvokeMoveNext`, this method takes an `IEnumerator`, which we can use to check which coroutine is running
- After the coroutines are all done running, execute our code
---
- [x] Check if the timing of the invoke is related with when the `yield` is supposed to be invoked
  - It does as expected and also as written on the unity execution order docs
- [x] Check if theres an alternative to `InvokeMoveNext` in early unity versions
  - There isn't, so instead we need to live patch `Class.<FooCoroutine>c__Iterator0.MoveNext()` when `StartCoroutine` is invoked
  - Careful not to patch something more than once as coroutine can be registered more than 1 time
- [x] Does timing of `MoveNext` in iterator match `UnityEngine.SetupCoroutine.InvokeMoveNext`
  - Yes

#### Info
- The method `MonoBehaviour.StartCoroutine(string, object)` and `MonoBehaviour.StartCoroutine(string)` will always search for a method returning `IEnumerator`
- **Order of selection**
  - declared order
  - number of args acending (0 or 1)
- If choosing the method without the `object` overload, it will use the first method that matches name and return type, with no argument
- If choosing the method with `object` overload, it will use the first method that matches name, return type, and if the `object` Type can be assigned to the argument Type
  - This will be done in declared order, so if you pass a `float` and methods for arguments `int` and `float` is declared in order, it will choose `int` because its assignable

# Implementation (of plan 3)
- Patch StartCoroutine methods
  - This patch will create a patch for the `IEnumerator.MoveNext` method if not already done for that method
  - We store the `IEnumerator` instance hash in a list somewhere on `StartCoroutine` invoke prefix
  - This list will also contain status for the instances
- When the `MoveNext` gets invoked on some type, monitor the return value
  - If `false`, means the iteration has finished and we remove from the hash list
  - Also check `Current` as it tells the type of wait we doing, and update the status of the tracker for this instance
- When the `MoveNext` is returning WaitForEndOfFrame, check status of all tracking instance
  - If there is any instance that hasn't been updated yet, don't execute final code
  - Else execute final update code
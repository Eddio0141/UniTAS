# Overview
I use lua for the movie, where defining movie properties also is done within the script itself

# Movie execution
## Config
- Because the config must be written somewhere, I've decided that if the script isn't `GLOBAL_SCOPE` then config values must be set before the first `yield`
- If `GLOBAL_SCOPE` then it just has to be set in the global scope with the appropriate name

## TAS execution
- The inputs and whatever you set before the first `yield` is the **first frame** and settings to be used
- If there is no `yield` at the final set of action, that section is the **last frame**
### Examples
```lua
-- some config
-- do whatever input stuff
i = 0
```
- This movie is 1f long
---
```lua
-- config
-- input stuff
adv()
-- more input stuff
```
- This movie is 2f long

# Config variables
- GLOBAL_SCOPE
  - Set to true and the main script's scope will be global. This means the user must manually return the coroutine that will be used to run the movie
- START_TIME
  - Either a DateTime or ticks for setting the start time as
- frametime / ft
  - Frametime for the initial game frametime, conflicts with fps, and with each other
- fps
  - Fps for the inital game fps, conflicts with frametime variables

## Processing config variables
### Idea
- Each config variable is a simple variable in the scope
- Each config variable is parsed in an unique way normally
- It needs to be gathered together in some type for convinient access

# Executing external methods
## Basic idea
Script engine will hold a list of pre-defined methods

Method defined will be an abstract class, where you can implement and handle the engine calling it
```cs
public abstract class EngineExternalMethod {
	public string Name { get; }
	public int ArgCount { get; }

	public EngineExternalMethod(name, argCount) {
		Name = name;
		ArgCount = argCount;
	}

	public abstract ICollection<ValueType> Invoke(ICollection<ValueType> args);
}
```

- In `Invoke`, returning value must be one of the engine types, optionally returning something
- Returning value could be a tuple, which you can create by returning a collection
- `ArgCount` is the number of arguments accepeted, and setting it to **-1** will let it take any number of arguments
---
Engine will then have to store the defined external method

```cs
// engine class
private const List<EngineExternalMethod> _methods = new() {
	new PrintToConsole(),
	new PressButton(),
	// ... more other stuff
};
```

## Examples
We have a method `PrintToConsole` in the plugin

### External method def
```cs
public class PrintToConsoleExternalMethod : EngineExternalMethod {
	public PrintToConsoleExternalMethod() : base("print_to_console", -1) {}

	public override ValueType? Invoke(ICollection<ValueType> args) {
		foreach (var arg in args) {
			PrintToConsole(arg.Value + "\n");
		}
		return null;
	}
}
```
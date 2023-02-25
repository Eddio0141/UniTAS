# Overview
I use lua for the movie, where defining movie properties also is done within the script itself

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
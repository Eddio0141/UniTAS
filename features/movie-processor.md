Movie processor functionality

# Overview
When the opcodes are ran for the current frame
- It will process the "main" sections of the opcodes such as the main scope, or subroutines
- It will process the concurrently running methods, which the script can subscribe with through built in method

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

	public abstract ValueType? Invoke(ICollection<ValueType> args);
}
```

- In `Invoke`, returning value must be one of the engine types, optionally returning something
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

### Add it to the engine
```cs
// engine class
private const List<EngineExternalMethod> _methods = new() {
	// ...,
	new PrintToConsoleExternalMethod()
};
```
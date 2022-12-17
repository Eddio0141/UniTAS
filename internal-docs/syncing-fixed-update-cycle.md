For some operations such as soft restart needs to wait for fixed update to sync

# How to do this
Introduce a class that will track sync with internal counters
```cs
class FixedUpdateTracker
```

Introduce a service that lets you register a callback method when sync happens
```cs
interface ISyncFixedUpdate {
	void OnSync(Action callback);
}
```
namespace UniTASPlugin.FakeGameState.GameFileSystem;

public abstract class Entry
{
    public string Name { get; protected set; }
    public Dir Parent { get; protected set; }
}

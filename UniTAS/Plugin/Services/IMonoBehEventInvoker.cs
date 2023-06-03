namespace UniTAS.Plugin.Services;

public interface IMonoBehEventInvoker
{
    void Update();
    void FixedUpdate();
    void LateUpdate();
    void OnGUI();
}
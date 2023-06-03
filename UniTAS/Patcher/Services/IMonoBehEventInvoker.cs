namespace UniTAS.Patcher.Services;

public interface IMonoBehEventInvoker
{
    void Update();
    void FixedUpdate();
    void LateUpdate();
    void OnGUI();
}
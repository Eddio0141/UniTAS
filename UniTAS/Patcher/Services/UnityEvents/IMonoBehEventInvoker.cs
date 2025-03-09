namespace UniTAS.Patcher.Services.UnityEvents;

public interface IMonoBehEventInvoker
{
    void InvokeAwake();
    void InvokeStart();
    void InvokeUpdate();
    void InvokeFixedUpdate();
    void InvokeLateUpdate();
    void InvokeOnGUI();
    void InvokeOnEnable();
    void InvokeEndOfFrame();
}
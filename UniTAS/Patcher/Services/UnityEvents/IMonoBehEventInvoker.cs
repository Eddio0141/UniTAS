namespace UniTAS.Patcher.Services.UnityEvents;

public interface IMonoBehEventInvoker
{
    void InvokeAwake();
    void InvokeStart();
    void InvokeUpdate();
    void InvokeFixedUpdate();
    void InvokeLateUpdate();
    void InvokeLastUpdate();
    void InvokeOnGUI();
    void InvokeOnEnable();
    void InvokeEndOfFrame();
}
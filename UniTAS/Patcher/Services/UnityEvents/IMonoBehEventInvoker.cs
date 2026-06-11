namespace UniTAS.Patcher.Services.UnityEvents;

public interface IMonoBehEventInvoker
{
    void InvokeAwake();
    void InvokeStart();
    void InvokeUpdate(bool monoBehCall);
    void InvokeFixedUpdate(bool monoBehCall);
    void InvokeLateUpdate(bool monoBehCall);
    void InvokeOnGUI();
    void InvokeOnEnable();
    void InvokeEndOfFrame();
}

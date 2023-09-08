namespace UniTAS.Patcher.Services;

public interface IMonoBehEventInvoker
{
    void Awake();
    void Start();
    void Update();
    void FixedUpdate();
    void LateUpdate();
    void OnGUI();
}
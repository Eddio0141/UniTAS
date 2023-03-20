namespace UniTAS.Plugin.Services;

public interface IMonoBehEventInvoker
{
    void Awake();
    void OnEnable();
    void Start();
    void Update();
    void LateUpdate();
    void FixedUpdate();
    void OnGUI();
}
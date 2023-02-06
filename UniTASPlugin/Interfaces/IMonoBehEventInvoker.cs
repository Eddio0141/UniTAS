namespace UniTASPlugin.Interfaces;

public interface IMonoBehEventInvoker
{
    void Awake();
    void OnEnable();
    void Start();
    void Update();
    void LateUpdate();
    void PreFixedUpdate();
    void FixedUpdate();
}
using UnityEngine;

public class AwakeTrack : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log($"Awake: {Time.frameCount}");
    }

    private void Start()
    {
        Debug.Log("Start");
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }
}

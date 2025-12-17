using UnityEngine;

public class A : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetAxis("Horizontal") != 0)
            Debug.Log("Horizontal");
        
        transform.position += transform.up * (Time.deltaTime * 0.1f);
    }

    private void OnBecameVisible()
    {
        Debug.Log($"visible: {Time.time}");
    }

    private void OnBecameInvisible()
    {
        Debug.Log($"invisible: {Time.time}");
    }
}

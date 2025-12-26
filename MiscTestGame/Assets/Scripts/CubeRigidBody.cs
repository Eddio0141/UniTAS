using UnityEngine;

public class CubeRigidBody : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _reportedResult;

    private void Awake()
    {
        Time.captureFramerate = 100;
    }

    private void Start()
    {
        Debug.Log(GetInstanceID());
        _rb = GetComponent<Rigidbody>();
        // -5.91, 2.311998, 108.44
        _rb.AddForce(transform.forward * 50, ForceMode.Impulse);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Instantiate(this);
        }

        if (_reportedResult || _rb.velocity.magnitude != 0) return;

        _reportedResult = true;
        Debug.Log(
            $"finished moving rigid body: position is now {transform.position.x:0.000000}, {transform.position.y:0.000000}, {transform.position.z:0.000000}");
    }
}
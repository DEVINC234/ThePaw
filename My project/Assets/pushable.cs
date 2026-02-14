using UnityEngine;

public class pushable : MonoBehaviour
{
    public float pushSpeed = 2f;

    private Rigidbody rb;
    private bool isBeingPushed;
    private Vector3 pushDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartPush(Vector3 direction)
    {
        isBeingPushed = true;
        pushDirection = direction;
    }

    public void StopPush()
    {
        isBeingPushed = false;
    }

    void FixedUpdate()
    {
        if (isBeingPushed)
        {
            rb.MovePosition(rb.position + pushDirection * pushSpeed * Time.fixedDeltaTime);
        }
    }
}

using UnityEngine;

public class fetchItem : MonoBehaviour
{
    public bool isHeld = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnPickedUp(Transform attachPoint)
    {
        isHeld = true;
        rb.isKinematic = true; // Stop physics so it doesn't fall
        transform.parent = attachPoint;
        transform.localPosition = Vector3.zero; // Snap to mouth
    }

    public void OnDropped()
    {
        isHeld = false;
        rb.isKinematic = false;
        transform.parent = null;
    }
}

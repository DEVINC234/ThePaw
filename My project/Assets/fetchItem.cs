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
        rb.isKinematic = true; 
        GetComponent<Collider>().enabled = false;
        transform.parent = attachPoint;
        transform.localPosition = Vector3.zero; 
    }

    public void OnDropped()
    {
        isHeld = false;
        rb.isKinematic = false;
        GetComponent<Collider>().enabled = true;
        transform.parent = null;
    }
}

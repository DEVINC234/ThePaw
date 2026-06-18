using UnityEngine;

public class GateKey : MonoBehaviour
{
    public bool isPickedUp = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetPickedUp(Transform parent, bool isDog)
    {
        isPickedUp = true;
        rb.isKinematic = true; 
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;

        transform.localRotation = isDog ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
    }

    public void Drop(Vector3 dropPosition)
    {
        isPickedUp = false;
        transform.SetParent(null);
        transform.position = dropPosition;
        rb.isKinematic = false;
        rb.AddForce(Vector3.down * 2f, ForceMode.Impulse); 
    }
}

using UnityEngine;

public class fetchKey : MonoBehaviour
{
    public Transform mouthSocket; 
    public bool hasKey = false;
    private GameObject carriedKey;
    public bool fetched = true;
    public CharacterSwitching Switching;
    public DogController dogController;
    void Start()
    {
       fetched = true;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key") && !hasKey)
        {
            hasKey = true;
            carriedKey = other.gameObject;

            carriedKey.GetComponent<Rigidbody>().isKinematic = true;
            carriedKey.transform.SetParent(mouthSocket);
            carriedKey.transform.localPosition = Vector3.zero;

            Debug.Log("Dog fetched the key!");
        }

       
        if (other.CompareTag("HandOffZone") && hasKey)
        {
            DropKey();
        }
    }

    void DropKey()
    {
        hasKey = false;
       
        carriedKey.transform.SetParent(null);
       
        carriedKey.transform.position += transform.forward * 0.5f;
        carriedKey.GetComponent<Rigidbody>().isKinematic = false;

        Debug.Log("Key dropped for the boy. Switching back...");

        Switching.ToggleCharacter(false);
        dogController.currentState = DogController.DogState.Follow;
    }
}

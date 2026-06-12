using UnityEngine;
using System.Collections;

public class GateUnlock : MonoBehaviour
{
    public bool isUnlocked = false;

    void OnCollisionEnter(Collision collision)
    {
    
        var boy = collision.gameObject.GetComponent<PlayerController>();

        if (boy != null && boy.isHoldingKey && !isUnlocked)
        {
            isUnlocked = true;
            StartCoroutine(OpenAndFinish());
        }
    }

    IEnumerator OpenAndFinish()
    {
       
        float elapsed = 0;
        Quaternion targetRot = transform.rotation * Quaternion.Euler(0, -90, 0);

        while (elapsed < 1.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("BOOM! Level Complete. Time for bed.");
        
    }
}

using UnityEngine;
using System.Collections;

public class GateRattle : MonoBehaviour
{
    [Header("Sequence Settings")]
    public Transform gateTopPoint;     
    public float lookUpDuration = 2.0f;
    public float shakeIntensity = 10f;
    public float shakeSpeed = 15f;

    [Header("References")]
    public CharacterSwitching switchManager;
    public GameObject mainCamera;      

    private bool hasAttempted = false;

   
    public void AttemptOpen(GameObject player, Animator playerAnim)
    {
        if (hasAttempted) return;
        hasAttempted = true;

        
        playerAnim.SetBool("Push", true);

      
        StartCoroutine(GateSequence(player, playerAnim));
    }

    IEnumerator GateSequence(GameObject player, Animator playerAnim)
    {

        Quaternion originalCamRot = mainCamera.transform.rotation;

        playerAnim.SetBool("Push", true);
        yield return new WaitForSeconds(1.5f);
        playerAnim.SetBool("Push", false);

        float elapsed = 0;
        while (elapsed < lookUpDuration)
        {
            Quaternion targetRot = Quaternion.LookRotation(gateTopPoint.position - mainCamera.transform.position);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, elapsed / lookUpDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(SimulateHeadShake(player.transform));

        elapsed = 0;
        while (elapsed < 1.0f) 
        {
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, originalCamRot, elapsed / 1.0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (switchManager != null)
        {
            switchManager.ToggleCharacter(true);
        }
    }

    IEnumerator SimulateHeadShake(Transform playerT)
    {
        Quaternion startRot = playerT.rotation;
        float elapsed = 0f;
        float duration = 1.2f;

        while (elapsed < duration)
        {

            float angle = Mathf.Sin(elapsed * shakeSpeed) * shakeIntensity;
            playerT.rotation = startRot * Quaternion.Euler(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerT.rotation = startRot; 
    }
}

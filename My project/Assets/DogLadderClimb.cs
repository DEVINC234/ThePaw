using UnityEngine;
using System.Collections;

public class DogLadderClimb : MonoBehaviour
{
    [Header("Teleport Destinations")]
    public Transform topOfLadderNode;    // Create an empty GameObject at the top floor
    public Transform bottomOfLadderNode; // Create an empty GameObject at the bottom floor

    [Header("Timing")]
    public float simulatedClimbTime = 1.5f; // The realistic delay while screen is black

    private bool isProcessingTeleport = false;

    void OnTriggerEnter(Collider other)
    {
        // Only trigger if the dog enters the zone and we aren't already mid-teleport
        if (other.CompareTag("Dog") && !isProcessingTeleport)
        {
            // Determine if the dog is at the bottom or top based on vertical height
            bool isAtBottom = other.transform.position.y < transform.position.y;
            Transform targetDestination = isAtBottom ? topOfLadderNode : bottomOfLadderNode;

            if (targetDestination != null)
            {
                StartCoroutine(LadderTeleportRoutine(other.gameObject, targetDestination));
            }
        }
    }

    IEnumerator LadderTeleportRoutine(GameObject dog, Transform destination)
    {
        isProcessingTeleport = true;

        // 1. Freeze the dog's input and movement engine immediately
        Rigidbody dogRb = dog.GetComponent<Rigidbody>();
        if (dogRb != null) dogRb.linearVelocity = Vector3.zero;

        // Disable the dog's movement controller script temporarily
        // dog.GetComponent<DogController>().enabled = false;

        // 2. Trigger your Camera Fade-Out
        // (Assuming you have a ScreenFade manager setup, or utilizing your Vignette setup)
        Debug.Log("Screen Fading Out...");
        // ScreenFadeManager.Instance.FadeToBlack(0.5f); 
        yield return new WaitForSeconds(0.5f); // Wait for screen to go pitch black

        // 3. Simulated Climb Time (The "Realistic" Delay)
        // Play a ladder scrambling audio clip here if you have one!
        Debug.Log("Dog is realistically climbing the ladder in the dark...");
        yield return new WaitForSeconds(simulatedClimbTime);

        // 4. Clean Teleport while the player can't see
        dog.transform.position = destination.position;
        dog.transform.rotation = destination.rotation;

        // Ensure physics doesn't carry old momentum over to the new floor
        if (dogRb != null) dogRb.linearVelocity = Vector3.zero;

        // 5. Trigger Camera Fade-In
        Debug.Log("Screen Fading Back In...");
        // ScreenFadeManager.Instance.FadeInFromBlack(0.5f);
        yield return new WaitForSeconds(0.5f);

        // 6. Restore control back to the player
        // dog.GetComponent<DogController>().enabled = true;

        isProcessingTeleport = false;
    }
}


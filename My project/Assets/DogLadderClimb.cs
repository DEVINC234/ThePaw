using UnityEngine;
using System.Collections;

public class DogLadderClimb : MonoBehaviour
{
    [Header("Teleport Destinations")]
    public Transform topOfLadderNode;    // Create an empty GameObject at the top floor
    public Transform bottomOfLadderNode; // Create an empty GameObject at the bottom floor

    [Header("Timing")]
    public float fadeDuration = 0.5f;       // Time it takes for the vignette to completely close/open
    public float simulatedClimbTime = 1.5f; // The realistic delay while screen is black

    [Header("Post-Processing Reference")]
    public PostProcessFader fader;          // Assign your PostProcessFader script here

    private bool isProcessingTeleport = false;

    void Start()
    {
        // Fallback: Try to find the fader in the scene if not assigned manually
        if (fader == null)
        {
            fader = FindObjectOfType<PostProcessFader>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Only trigger if the dog enters the zone and we aren't already mid-teleport
        if (other.CompareTag("Dog") && !isProcessingTeleport)
        {
            // Determine if the dog is at the bottom or top based on vertical height
            bool isAtBottom = other.transform.position.y < transform.position.y;
            Transform targetDestination = isAtBottom ? topOfLadderNode : bottomOfLadderNode;

            if (targetDestination != null && fader != null)
            {
                StartCoroutine(LadderTeleportRoutine(other.gameObject, targetDestination));
            }
            else if (fader == null)
            {
                Debug.LogWarning("Cannot climb ladder: PostProcessFader reference is missing!");
            }
        }
    }

    IEnumerator LadderTeleportRoutine(GameObject dog, Transform destination)
    {
        isProcessingTeleport = true;

        // 1. Freeze the dog's input and movement engine immediately
        Rigidbody dogRb = dog.GetComponent<Rigidbody>();
        if (dogRb != null) dogRb.linearVelocity = Vector3.zero;

        // Disable the dog's movement controller script temporarily so player input cuts out
        DogController dogController = dog.GetComponent<DogController>();
        if (dogController != null) dogController.enabled = false;

        // 2. Trigger Post-Processing Cinematic Fade-Out
        Debug.Log("Screen Fading Out via Vignette...");
        yield return StartCoroutine(fader.FadeToBlack(fadeDuration));

        // 3. Simulated Climb Time (The "Realistic" Delay while screen is black)
        Debug.Log("Dog is realistically climbing the ladder in the dark...");
        yield return new WaitForSeconds(simulatedClimbTime);

        // 4. Clean Teleport while the player can't see anything
        dog.transform.position = destination.position;
        dog.transform.rotation = destination.rotation;

        // Ensure physics doesn't carry old momentum over to the new floor
        if (dogRb != null) dogRb.linearVelocity = Vector3.zero;

        // 5. Trigger Post-Processing Cinematic Fade-In
        Debug.Log("Screen Fading Back In via Vignette...");
        yield return StartCoroutine(fader.FadeInFromBlack(fadeDuration));

        // 6. Restore control back to the player
        if (dogController != null) dogController.enabled = true;

        isProcessingTeleport = false;
    }
}


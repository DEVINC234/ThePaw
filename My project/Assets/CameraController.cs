using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("Base Offset")]
    public Vector3 offset;

    [Header("X Offset Values")]
    public float idleX = -3.5f;
    public float moveX = -4.3f;

    [Header("Speeds")]
    public float followSpeed = 5f;
    public float shiftSpeed = 3f;

    [Header("Post-Processing Intro Fade")]
    public PostProcessVolume postProcessVolume; 
    public float fadeInDuration = 2.5f;         

    private Vignette vignette;
    private Vector3 lastPlayerPos;
    private bool isIntroFading = true;

    void Start()
    {
        lastPlayerPos = player.position;

        // Try to grab the Vignette settings out of your Post-Process Volume profile
        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out vignette))
        {
            // Lock the screen to pitch black immediately at launch
            vignette.intensity.Override(1f);

            // Kick off the smooth fade-in sequence
            StartCoroutine(IntroFadeInRoutine());
        }
        else
        {
            Debug.LogWarning("Vignette settings missing from PostProcess Volume profile!");
            isIntroFading = false;
        }
    }

    IEnumerator IntroFadeInRoutine()
    {
        float elapsed = 0f;
        float startIntensity = 3f;
        float targetIntensity = 0.25f; 

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;

            // Calculate smooth transition over time
            float currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / fadeInDuration);
            vignette.intensity.Override(currentIntensity);

            yield return null;
        }

        // Lock it to the finalized game look
        vignette.intensity.Override(targetIntensity);
        isIntroFading = false;
    }

    void LateUpdate()
    {
        // Smoothly follow player even during a fade sequence
        float movement = Vector3.Distance(player.position, lastPlayerPos);
        float targetX = movement > 0.01f ? moveX : idleX;

        // Smoothly change X offset
        offset.x = Mathf.Lerp(offset.x, targetX, shiftSpeed * Time.deltaTime);

        // Follow player
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        lastPlayerPos = player.position;
    }
}

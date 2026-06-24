using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PostProcessFader : MonoBehaviour
{
    public PostProcessVolume targetVolume;
    private Vignette vignette;

    void Start()
    {
        // Grab the Vignette settings from your profile dynamically
        if (targetVolume != null)
        {
            targetVolume.profile.TryGetSettings(out vignette);
        }
    }

    // Call this from your Dog's ladder trigger sequence
    public IEnumerator FadeToBlack(float duration)
    {
        if (vignette == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Linearly interpolate vignette intensity from normal gameplay to total pitch black
            vignette.intensity.value = Mathf.Lerp(0f, 1f, elapsed / duration);
            vignette.smoothness.value = Mathf.Lerp(0.2f, 1f, elapsed / duration);
            yield return null;
        }

        // Lock it at absolute black
        vignette.intensity.value = 1f;
        vignette.smoothness.value = 1f;
    }

    public IEnumerator FadeInFromBlack(float duration)
    {
        if (vignette == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Fade it back out so the world becomes visible again
            vignette.intensity.value = Mathf.Lerp(1f, 0f, elapsed / duration);
            vignette.smoothness.value = Mathf.Lerp(1f, 0.2f, elapsed / duration);
            yield return null;
        }

        // Restore default gameplay visual weight
        vignette.intensity.value = 0f;
        vignette.smoothness.value = 0.2f;
    }
}

using UnityEngine;
using TMPro;
using System.Collections;

public class TriggerUITextFade : MonoBehaviour
{
    public CanvasGroup textCanvasGroup;
    public float fadeDuration = 0.5f;
    public float displayDuration = 3f;

    private bool hasTriggered = false;
    private Coroutine fadeCoroutine;

    // Static reference to the currently active fade script
    private static TriggerUITextFade currentlyActive;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            // If another text is already active, stop it and hide its text
            if (currentlyActive != null && currentlyActive != this)
            {
                currentlyActive.StopAllCoroutines();
                currentlyActive.textCanvasGroup.alpha = 0f;
            }

            currentlyActive = this;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeInAndOut());
        }
    }

    private IEnumerator FadeInAndOut()
    {
        // Fade in
        yield return StartCoroutine(FadeText(1f));

        // Wait while visible
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeText(0f));

        // Clear current reference after fading out
        if (currentlyActive == this)
            currentlyActive = null;
    }

    private IEnumerator FadeText(float targetAlpha)
    {
        float startAlpha = textCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        textCanvasGroup.alpha = targetAlpha;
    }
}

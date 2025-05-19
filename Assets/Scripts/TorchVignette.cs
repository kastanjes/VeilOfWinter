using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TorchVignette : MonoBehaviour
{
    public CanvasGroup vignetteImageOpacity;
    public Image vignetteImage;
    public GameObject playerTorch;

    public Vector3 vignetteStartScale;
    public Vector3 vignetteEndScale = new Vector3(0.5f, 0.5f, 0.5f); // Adjust as needed
    public float vignetteFadeDuration = 3f;

    private Coroutine currentVignetteRoutine;

    void Start()
    {
        // Store the starting scale from the UI Image
        vignetteStartScale = vignetteImage.rectTransform.localScale;
    }


    public void ResetVignette()
    {
        if (currentVignetteRoutine != null)
            StopCoroutine(currentVignetteRoutine);

        vignetteImage.rectTransform.localScale = vignetteStartScale;
        vignetteImageOpacity.alpha = 1f; // Optional: reset visibility
    }

    public void StartVignetteScaling(float duration)
{
    if (currentVignetteRoutine != null)
        StopCoroutine(currentVignetteRoutine);

    currentVignetteRoutine = StartCoroutine(ScaleVignette(vignetteStartScale, vignetteEndScale, duration));
}

    private IEnumerator ScaleVignette(Vector3 from, Vector3 to, float duration)
    {

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            vignetteImage.rectTransform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        vignetteImage.rectTransform.localScale = to;
    
}

}

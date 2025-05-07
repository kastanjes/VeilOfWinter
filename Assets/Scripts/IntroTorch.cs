using System.Collections;
using UnityEngine;

public class IntroTorchController : MonoBehaviour
{
    public ParticleSystem torchParticles;
    public float fadeDuration = 1.5f;
    private bool isFading = false;

    void Start()
    {
        if (torchParticles == null)
            torchParticles = GetComponent<ParticleSystem>();
    }

    public void FadeOutTorch()
    {
        if (!isFading)
            StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        isFading = true;

        var main = torchParticles.main;
        float startSize = main.startSize.constant;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float newSize = Mathf.Lerp(startSize, 0, t / fadeDuration);
            main.startSize = newSize;
            yield return null;
        }

        main.startSize = 0;
        torchParticles.Stop();
        isFading = false;
    }
}

using System.Collections;
using UnityEngine;

public class IntroTorchController : MonoBehaviour
{
    public ParticleSystem[] torchParticles; // Support multiple systems
    public float fadeDuration = 1.5f;
    private bool isFading = false;

    void Start()
    {
        if (torchParticles == null || torchParticles.Length == 0)
            torchParticles = GetComponentsInChildren<ParticleSystem>();
    }

    public void FadeOutTorch()
    {
        if (!isFading)
            StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        isFading = true;

        float[] startSizes = new float[torchParticles.Length];

        for (int i = 0; i < torchParticles.Length; i++)
        {
            startSizes[i] = torchParticles[i].main.startSize.constant;
        }

        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            for (int i = 0; i < torchParticles.Length; i++)
            {
                var main = torchParticles[i].main;
                float newSize = Mathf.Lerp(startSizes[i], 0, t / fadeDuration);
                main.startSize = newSize;
            }
            yield return null;
        }

        foreach (var ps in torchParticles)
        {
            var main = ps.main;
            main.startSize = 0;
            ps.Stop();
        }

        isFading = false;
    }
}

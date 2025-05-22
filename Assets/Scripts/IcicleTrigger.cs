using UnityEngine;
using System.Collections;

public class IcicleTrigger : MonoBehaviour
{
    private Rigidbody parentRb;
    private bool hasFallen = false;

    [Header("Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeAmount = 0.001f;
    private Vector3 originalPosition;

    [Header("Audio Settings")]
    public float volume = 1.0f;
    private AudioSource audioSource;
    private static AudioClip dropSound;

    private static bool dropSoundIsPlaying = false; // 👈 prevent overlap

    void Start()
    {
        parentRb = GetComponentInParent<Rigidbody>();
        originalPosition = parentRb != null ? parentRb.transform.localPosition : Vector3.zero;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (dropSound == null)
        {
            dropSound = Resources.Load<AudioClip>("Audio/SFX - Icicles Break Drop (Christmas)");
            if (dropSound == null)
                Debug.LogError("Drop sound not found in Resources/Audio folder.");
        }
    }

private void OnTriggerEnter(Collider other)
{
    if (!hasFallen && other.CompareTag("Player"))
    {
        hasFallen = true;

        // 🎵 Play sound immediately
        if (!dropSoundIsPlaying && dropSound != null)
        {
            dropSoundIsPlaying = true;
            audioSource.PlayOneShot(dropSound, volume);
            StartCoroutine(ResetDropSoundFlag(dropSound.length));
        }

        StartCoroutine(ShakeAndDrop());
    }
}

private IEnumerator ShakeAndDrop()
{
    float elapsed = 0f;

    while (elapsed < shakeDuration)
    {
        elapsed += Time.deltaTime;
        Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
        parentRb.transform.localPosition = originalPosition + randomOffset;
        yield return null;
    }

    parentRb.transform.localPosition = originalPosition;

    parentRb.useGravity = true;
    parentRb.isKinematic = false;
}

private IEnumerator ResetDropSoundFlag(float delay)
{
    yield return new WaitForSeconds(delay);
    dropSoundIsPlaying = false;
}

}


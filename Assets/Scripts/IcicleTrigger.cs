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

    void Start()
    {
        parentRb = GetComponentInParent<Rigidbody>();

        if (parentRb == null)
        {
            Debug.LogError("IcicleTrigger: No Rigidbody found on parent object!");
        }

        originalPosition = parentRb.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasFallen && other.CompareTag("Player"))
        {
            hasFallen = true;
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

        // Reset position before drop
        parentRb.transform.localPosition = originalPosition;

        // Enable gravity
        parentRb.useGravity = true;
        parentRb.isKinematic = false;
    }
}

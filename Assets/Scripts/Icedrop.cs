using System.Collections;
using UnityEngine;

public class IceDrop : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasDropped = false;

    public AudioClip dropSound;
    public AudioSource audioSource;

    public GameObject shatterParticles;

    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
    }

    public void Drop()
    {
        if (hasDropped) return;

        hasDropped = true;
        StartCoroutine(StartDrop());
    }

    private IEnumerator StartDrop()
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasDropped) return;

        if (dropSound != null && audioSource != null && !audioSource.isPlaying)
            audioSource.PlayOneShot(dropSound);

        if (shatterParticles != null)
        {
            GameObject p = Instantiate(shatterParticles, transform.position, Quaternion.identity);
            Destroy(p, 2f);
        }

        Destroy(gameObject, 0.5f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class IceDrop : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasDropped = false;

    public AudioClip dropSound;
    private AudioSource audioSource;

    public GameObject shatterParticles; 

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

        rb.useGravity = true;
        hasDropped = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Instantiating shatter effect");
        if (!hasDropped) return;

        if (dropSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(dropSound);
        }

        if (shatterParticles != null)
        {
            GameObject p = Instantiate(shatterParticles, transform.position, Quaternion.identity);
            Destroy(p, 2f); 
        }

        Destroy(gameObject, 0.5f); 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IceTrigger : MonoBehaviour
{
    public GameObject iceObject;
    public AudioClip dropSound;
    public GameObject impactEffectPrefab;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IceDrop dropScript = iceObject.GetComponent<IceDrop>();
            if (dropScript != null)
            {
                dropScript.Drop();
            }

            if (dropSound != null)
            {
                audioSource.PlayOneShot(dropSound);
            }

            Debug.Log("Jakob lugter af pølse");
        }
    }
}


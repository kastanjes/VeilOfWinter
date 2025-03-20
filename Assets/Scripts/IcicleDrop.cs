using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleDrop : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasDropped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;  // Keep it static at start
        rb.gravityScale = 0;    // Disable gravity at the start
    }

    public void Drop()
    {
        Debug.Log("Drop() method called!"); // Check if this appears in the Console

        if (!hasDropped)
        {
            StartCoroutine(ShakeBeforeDrop()); // Start shaking before dropping
        }
    }

    IEnumerator ShakeBeforeDrop()
    {
        Vector3 originalPos = transform.position;

        // Shake effect before falling
        for (int i = 0; i < 10; i++)
        {
            transform.position = originalPos + new Vector3(Random.Range(-0.1f, 0.1f), 0, 0);
            yield return new WaitForSeconds(0.05f);
        }

        transform.position = originalPos; // Reset position after shaking

        // Allow icicle to fall
        rb.isKinematic = false;
        rb.gravityScale = 1;
        hasDropped = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject, 1f); // Destroy after 1 second
        }
    }
}



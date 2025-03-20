using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleTrigger : MonoBehaviour
{
    public IcicleDrop icicle; // Assign in Unity Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger Entered by: " + other.gameObject.name); // ✅ See if the trigger detects something

        if (other.CompareTag("Player")) // ✅ Make sure the player is correctly tagged
        {
            Debug.Log("Player entered trigger zone! Calling Drop()");
            icicle.Drop(); // Call the Drop method
        }
    }
}

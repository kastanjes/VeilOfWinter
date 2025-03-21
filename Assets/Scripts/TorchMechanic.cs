using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchMechanic : MonoBehaviour
{
    public Light torch;
    public float startIntensity;
    public int fadeTime = 10;

    private bool isLit = true;
    private float fadeRate;
    private bool canPickupTorch = false;

    private GameObject torchPrefab; 

    void Start()
    {
        if (torch == null)
            torch = GetComponent<Light>();

        startIntensity = torch.intensity;
        fadeRate = startIntensity / fadeTime;
    }

    void Update()
    {
        if (isLit)
        {
            Fading();
        }

        if (canPickupTorch && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Torch picked up! Destroying collectible torch.");
            TorchPickup();
            Destroy(torchPrefab);
        }
    }

    public void Fading()
    {
        if (torch.intensity > 0)
        {
            torch.intensity -= fadeRate * Time.deltaTime;

            if (torch.intensity <= 0)
            {
                torch.intensity = 0;
                torch.enabled = false;
                isLit = false;
                Debug.Log("Torch faded out.");
            }
        }
    }

    public void TorchPickup()
    {
        torch.enabled = true;
        torch.intensity = startIntensity;
        isLit = true;
        Debug.Log("Torch re-lit");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Torch")) 
        {
            canPickupTorch = true;
            torchPrefab = other.gameObject;
            Debug.Log("Player entered pickup range of a torch.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Torch"))
        {
            canPickupTorch = false;
            torchPrefab = null;
            Debug.Log("Player exited pickup range of a torch.");
        }
    }
}

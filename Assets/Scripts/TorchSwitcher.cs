using UnityEngine;
using System.Collections;

public class TorchSwitcher : MonoBehaviour
{
    public GameObject introTorch;
    public GameObject playerTorch;

    public void SwapTorches()
    {
        if (introTorch != null) introTorch.SetActive(false);
        
        if (playerTorch != null)
        {
            playerTorch.SetActive(true);

            // Disable the particle system after activation
            var particles = playerTorch.GetComponentInChildren<ParticleSystem>();
            if (particles != null) particles.Stop();
        }
    }

    // Final position logic removed — no longer needed
}

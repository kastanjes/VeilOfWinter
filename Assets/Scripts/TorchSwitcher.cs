using UnityEngine;
using System.Collections;

public class TorchSwitcher : MonoBehaviour
{
    public GameObject introTorch;
    public GameObject playerTorch;

    [Header("Player Positioning")]
    public Transform finalPlayerPosition;
    public GameObject player;

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

public void MovePlayerToFinalPosition()
{
    StartCoroutine(DelayedMove());
}

private IEnumerator DelayedMove()
{
    // Wait until the animation finishes
    yield return new WaitForEndOfFrame();

    Animator animator = player.GetComponent<Animator>();
    if (animator != null)
    {
        animator.enabled = false; // Temporarily disable Animator
    }

    if (player != null && finalPlayerPosition != null)
    {
        player.transform.position = finalPlayerPosition.position;
        player.transform.rotation = finalPlayerPosition.rotation; // Optional
    }
    else
    {
        Debug.LogWarning("Player or final position is not assigned.");
    }

    // (Optional) Re-enable Animator if needed:
    // yield return null;
    // if (animator != null) animator.enabled = true;
}


}

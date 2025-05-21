using System.Collections;
using UnityEngine;
using System.Linq;

public class TorchMechanic : MonoBehaviour
{
    public ParticleSystem[] torchParticles; // Array of 3 particle systems
    public float startSize = 1f;
    public int fadeTime = 10;

    private bool isLit = true;
    private float fadeRate;
    private bool canPickupTorch = false;

    private GameObject torchInRange;
    private bool isPickingUp = false;

    public GameObject objectToDisableOnFirstLight; // Assign in inspector
private static bool torchLitOnce = false; // Tracks if it's the first time


    void Start()
    {
        if (torchParticles == null || torchParticles.Length == 0)
        {
            torchParticles = GetComponentsInChildren<ParticleSystem>();
        }

        // Use the first system as reference
        var main = torchParticles[0].main;
        startSize = main.startSize.constant;
        fadeRate = startSize / fadeTime;
    }

    void Update()
    {
        if (isLit)
        {
            Fading();
        }

        if (!canPickupTorch) return;

        if (canPickupTorch && Input.GetKeyDown(KeyCode.E) && !isPickingUp)
        {
            Debug.Log("E pressed near torch. Starting pickup.");
            StartCoroutine(PickupTorchCoroutine());
        }
    }

    IEnumerator PickupTorchCoroutine()
    {
        isPickingUp = true;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            CharacterMovement movement = player.GetComponent<CharacterMovement>();
            Animator animator = player.GetComponent<Animator>();

            if (movement != null)
            {
                movement.canMove = false;
                movement.StartTorchPickup();
            }

            if (animator != null)
            {
                animator.ResetTrigger("TorchTrigger");
                animator.SetTrigger("TorchTrigger");
            }

            yield return new WaitForSeconds(2f);

            TorchPickup();

            if (torchInRange != null)
            {
                Destroy(torchInRange);
                torchInRange = null;
                canPickupTorch = false;
            }

            if (movement != null)
            {
                movement.canMove = true;
                movement.EndTorchPickup();
            }

            Debug.Log("Torch pickup complete.");
        }

        isPickingUp = false;
    }

    public void Fading()
    {
        var main = torchParticles[0].main;

        if (main.startSize.constant > 0)
        {
            float newSize = Mathf.Max(0, main.startSize.constant - fadeRate * Time.deltaTime);

            foreach (var ps in torchParticles)
            {
                var m = ps.main;
                m.startSize = newSize;
                if (newSize <= 0) ps.Stop();
            }

            if (newSize <= 0)
            {
                isLit = false;
                Debug.Log("Torch faded out.");

            if (EndSceneTrigger.playerEnteredEndZone)
            {
                Debug.Log("Torch faded out in end zone. Loading end cutscene scene...");
                SceneLoader.Instance.LoadScene("EndScene"); // ✅ change scene name if needed
            }
            else if (RespawnManager.Instance != null && RespawnManager.Instance.HasRespawnPoint())
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    var movement = player.GetComponent<CharacterMovement>();
                    if (movement != null) movement.DieAndRespawn();
                }
            }

            }
        }
    }

    public void TorchPickup()
    {
        TorchVignette vignette = FindObjectOfType<TorchVignette>();
        if (vignette != null)
        {
            vignette.StartVignetteScaling(fadeTime);
        }

        foreach (var ps in torchParticles)
        {
            ps.Play();
            var m = ps.main;
            m.startSize = startSize;
        }

        isLit = true;
        if (!torchLitOnce)
{
    torchLitOnce = true;

    if (objectToDisableOnFirstLight != null)
    {
        objectToDisableOnFirstLight.SetActive(false);
        Debug.Log("Disabled first-time object after relighting torch.");
    }
}


        if (RespawnManager.Instance != null && torchInRange != null)
        {
            // 🔧 Get the RespawnPoint inside the picked-up torch
            Transform marker = torchInRange.transform.Find("RespawnPoint");
            if (marker == null)
            {
                // Try a deep search just in case
                marker = torchInRange.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "RespawnPoint");
            }

            Vector3 target = marker != null ? marker.position : torchInRange.transform.position;
            Vector3 groundedPosition = RaycastToGround(target);
            RespawnManager.Instance.SetRespawnPoint(groundedPosition);

            Debug.Log($"Respawn point set to: {groundedPosition}");
        }

        Debug.Log("Torch re-lit and respawn point set.");
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CollectableTorch"))
        {
            canPickupTorch = true;
            torchInRange = other.gameObject;
            Debug.Log("Collectable torch in range.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CollectableTorch"))
        {
            canPickupTorch = false;
            torchInRange = null;
            Debug.Log("Left collectable torch range.");
        }
    }

    private Vector3 RaycastToGround(Vector3 origin)
    {
        int groundLayerMask = LayerMask.GetMask("Ground");
        Ray ray = new Ray(origin + Vector3.up * 1f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 5f, groundLayerMask))
        {
            if (hitInfo.collider.CompareTag("Ground"))
            {
                return hitInfo.point;
            }
        }

        Debug.LogWarning("No valid ground hit for respawn. Falling back to torch position.");
        return origin;
    }
    
    public void ReactivateTorchParticles()
{
    foreach (var ps in torchParticles)
    {
        ps.Play();
        var m = ps.main;
        m.startSize = startSize;
    }

    isLit = true;
    Debug.Log("Torch particles reactivated after respawn.");
}

}
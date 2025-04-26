using System.Collections;
using UnityEngine;

public class TorchMechanic : MonoBehaviour
{
    public Light torch;
    public float startIntensity;
    public int fadeTime = 10;

    private bool isLit = true;
    private float fadeRate;
    private bool canPickupTorch = false;

    private GameObject torchInRange;
    private bool isPickingUp = false;

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

            if (movement != null) movement.canMove = false;

            if (animator != null)
            {
                animator.ResetTrigger("TorchTrigger");
                animator.SetTrigger("TorchTrigger");
            }

            yield return new WaitForSeconds(2f);

            TorchPickup();

            if (torchInRange != null)
                Destroy(torchInRange);

            if (movement != null)
                movement.canMove = true;

            Debug.Log("Torch pickup complete.");
        }

        isPickingUp = false;
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
}

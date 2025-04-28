using System.Collections;
using UnityEngine;

public class TorchMechanic : MonoBehaviour
{
    public ParticleSystem torchParticles;  // Reference to the Particle System
    public float startSize;               // Starting size of the particles
    public int fadeTime = 10;             // Time to fade out the particles

    private bool isLit = true;
    private float fadeRate;
    private bool canPickupTorch = false;

    private GameObject torchInRange;
    private bool isPickingUp = false;

    void Start()
    {
        if (torchParticles == null)
            torchParticles = GetComponent<ParticleSystem>();  // Get the Particle System attached to the GameObject

        var main = torchParticles.main;
        startSize = main.startSize.constant;  // Get the starting size of the particles
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
                movement.StartTorchPickup(); // <<< THIS makes the Rigidbody kinematic
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
                canPickupTorch = false; // <<< Add this line
            }

            if (movement != null)
            {
                movement.canMove = true;
                movement.EndTorchPickup(); // <<< THIS restores the Rigidbody to normal
            }

            Debug.Log("Torch pickup complete.");
        }

        isPickingUp = false;
    }

    public void Fading()
    {
        var main = torchParticles.main;

        if (main.startSize.constant > 0)
        {
            main.startSize = Mathf.Max(0, main.startSize.constant - fadeRate * Time.deltaTime);

            if (main.startSize.constant <= 0)
            {
                main.startSize = 0;
                torchParticles.Stop();
                isLit = false;
                Debug.Log("Torch faded out.");
            }
        }
    }

    public void TorchPickup()
    {
        torchParticles.Play();  // Play the particles when the torch is lit
        var main = torchParticles.main;
        main.startSize = startSize;  // Reset the particle size to the start value
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

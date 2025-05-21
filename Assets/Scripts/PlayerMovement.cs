using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    CharacterAnimation characterAnimation;

    // Erstat AudioManager referencer med AudioSource
    [SerializeField] private AudioSource footstepsAudioSource;

    [Header("Movement Settings")]
    public bool canMove = true;
    public float maxMoveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    [Header("Wind Settings")]
    public float windBackwardsForce = 15f;
    public float forwardJumpForceReduction = 0.7f;
    // Nye variabler for crouch
    public bool canCrouch = true;
    public float crouchWindResistance = 0.05f; // Kun 5% vindpåvirkning når crouched

    [Header("Ice Settings")]
    public float iceSpeedMultiplier = 1.8f;
    public float iceSlideFactor = 0.95f;
    public string iceSurfaceTag = "Ice";

    [Header("UI")]
    public CanvasGroup blackOverlay;
    public float fadeDuration = 1f;


    private bool isGrounded;
    private bool jumpTriggered = false;
    private bool isInWindGust = false;
    private bool isOnIce = false;
    private bool windAnimationTriggered = false;
    private WindZone windZone;
    private Vector3 slidingDirection;
    private bool isCrouching = false; // Crouch-tilstand

    private bool isPickingUpTorch = false;

    private bool wasMovingLastFrame = false;
    private Quaternion lastRotationBeforeStop;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        characterAnimation = GetComponent<CharacterAnimation>();

        windZone = FindObjectOfType<WindZone>();
        slidingDirection = Vector3.zero;

        // Tjek om AudioSource er tildelt
        if (footstepsAudioSource == null)
        {
            Debug.LogError("Footsteps AudioSource er ikke tildelt! Tilføj en AudioSource komponent til spilleren og træk den til dette felt i inspektoren.");
            // Forsøg at finde eller tilføje en
            footstepsAudioSource = GetComponent<AudioSource>();
            if (footstepsAudioSource == null)
            {
                footstepsAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("AudioSource automatisk tilføjet til spilleren.");
            }
        }

        // Sørg for at AudioSource er konfigureret korrekt
        footstepsAudioSource.loop = true;
        footstepsAudioSource.playOnAwake = false;
    }

    void Update()
    {
        if (!canMove) return;

        // Ground check
        CheckGroundedAndSurface();

        // Wind gust check
        if (windZone != null)
            isInWindGust = windZone.windMain > 10.0f;

        if (!isInWindGust)
            windAnimationTriggered = false;

        // Crouch håndtering med korrekt animation parameter
        if (Input.GetKeyDown(KeyCode.LeftControl) && canCrouch && isGrounded)
        {
            isCrouching = true;
            animator.SetBool("IsCrouching", true);
            Debug.Log("CROUCH AKTIVERET");
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && canCrouch)
        {
            isCrouching = false;
            animator.SetBool("IsCrouching", false);
            Debug.Log("CROUCH DEAKTIVERET");
        }

        // Jump - kan ikke hoppe mens crouched
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !jumpTriggered && !isCrouching)
        {
            HandleJumping();
        }
    }

    void FixedUpdate()
    {
        if (!canMove || isPickingUpTorch) return;
        Debug.Log($"canMove={canMove}, velocity={rb.velocity}, grounded={isGrounded}");


        float forward = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float sideways = Input.GetKey(KeyCode.S) ? 1 : Input.GetKey(KeyCode.W) ? -1 : 0;

        Vector3 moveDirection = new Vector3(sideways, 0, forward).normalized;
        float actualSpeed = maxMoveSpeed * characterAnimation.velocity;

        // Helt ny vindlogik med stærk reduktion for crouch
        if (isInWindGust && windZone != null)
        {
            // Drastisk reduktion af vindkraft når crouched
            if (isCrouching && isGrounded)
            {
                // Næsten ingen vindpåvirkning når crouched
                float crouchedWindForce = crouchWindResistance; // Meget lav værdi

                // Anvend minimal vindkraft
                rb.AddForce(windZone.transform.forward * -crouchedWindForce, ForceMode.Force);
                Debug.Log("CROUCH VINDMODSTAND: Kraft reduceret til " + crouchedWindForce);

                // Ingen vindanimation ved crouch
                windAnimationTriggered = false;
            }
            else
            {
                // Normal vindkraft når ikke crouched
                float normalWindForce = isGrounded ? 2f : 4f;

                // Anvend normal vindkraft
                rb.AddForce(windZone.transform.forward * -normalWindForce, ForceMode.Force);

                // Vis kun wind animation hvis ikke crouched
                if (moveDirection.magnitude < 0.1f && isGrounded && !windAnimationTriggered)
                {
                    // animator.SetTrigger("Wind");
                    Debug.Log("Player hit by wind");
                    windAnimationTriggered = true;
                }
            }
        }
        else
        {
            if (isOnIce)
            {
                ApplyIceMovement(moveDirection, actualSpeed);
            }
            else
            {
                ApplyNormalMovement(moveDirection, actualSpeed);
            }
        }

        // NY FODTRIN LOGIK - mere direkte kontrol
        bool hasMovementInput = moveDirection.magnitude > 0.1f;
        bool shouldPlayFootsteps = isGrounded && !jumpTriggered && hasMovementInput && !isPickingUpTorch && !isOnIce && !isCrouching;

        // Kontrollerer fodtrinslyd baseret på bevægelse
        if (shouldPlayFootsteps)
        {
            if (!footstepsAudioSource.isPlaying)
            {
                footstepsAudioSource.Play();
                Debug.Log("Fodtrin starter");
            }
        }
        else
        {
            if (footstepsAudioSource.isPlaying)
            {
                footstepsAudioSource.Stop();
                Debug.Log("Fodtrin stopper");
            }
        }

        bool isCurrentlyMoving = moveDirection.magnitude >= 0.1f;

        if (isCurrentlyMoving)
        {
            // Rotation while moving
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            wasMovingLastFrame = true;
        }
        else
        {
            // Just stopped moving
            if (wasMovingLastFrame)
            {
                lastRotationBeforeStop = transform.rotation;
                wasMovingLastFrame = false;
            }

            // Keep last facing direction
            transform.rotation = lastRotationBeforeStop;
        }



    }

    private void CheckGroundedAndSurface()
    {
        // Brug OverlapSphere for mere pålidelig detektion
        Vector3 spherePosition = transform.position - new Vector3(0, groundCheckDistance / 2, 0);
        float sphereRadius = 0.3f;

        // Få alle colliders inden for sfæren
        Collider[] hitColliders = Physics.OverlapSphere(spherePosition, sphereRadius, groundLayer);

        // Nulstil status
        isGrounded = hitColliders.Length > 0;
        isOnIce = false;

        if (isGrounded)
        {
            // Check hvert objekt for is-tag
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag(iceSurfaceTag))
                {
                    isOnIce = true;
                    break;
                }
            }

            jumpTriggered = false;
        }
    }

    private void HandleJumping()
    {
        // Simple grundlæggende hop kraft
        Vector3 jumpVector = Vector3.up * jumpForce;

        // Anvend altid basis-hop kraften
        rb.AddForce(jumpVector, ForceMode.Impulse);

        // Hvis vi er i et vindstød
        if (isInWindGust && windZone != null)
        {
            // Dette er i verdenskoordinater - ikke relateret til spillerens rotation
            Vector3 worldBackward = new Vector3(0, 0, -1); // Baglæns på z-aksen

            // Anvendt som en ekstrem kraft
            rb.AddForce(worldBackward * windBackwardsForce * 2.0f, ForceMode.Impulse);
        }

        // Trigger animation
        animator.ResetTrigger("JumpTrigger");
        animator.SetTrigger("JumpTrigger");
        jumpTriggered = true;
    }

    private void ApplyNormalMovement(Vector3 moveDirection, float speed)
    {
        // Reducér hastigheden når crouched
        if (isCrouching)
            speed *= 0.5f; // Halv hastighed mens crouched

        Vector3 moveVelocity = moveDirection * speed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        slidingDirection = Vector3.zero;
    }

    private void ApplyIceMovement(Vector3 moveDirection, float speed)
    {
        // Reducér hastigheden når crouched (også på is)
        if (isCrouching)
            speed *= 0.5f;

        if (moveDirection.magnitude > 0.1f)
        {
            // Gradvis ændring af glidningsretning baseret på input
            float lerpFactor = 1 - iceSlideFactor; // 0.05 hvis iceSlideFactor er 0.95
            slidingDirection = Vector3.Lerp(slidingDirection, moveDirection, lerpFactor);
        }
        else if (slidingDirection.magnitude > 0.01f)
        {
            // Gradvis aftagen af glidning når der ikke er input
            slidingDirection = Vector3.Lerp(slidingDirection, Vector3.zero, Time.deltaTime * 0.3f);
        }

        // Anvend hastighedsmodifikatorer
        float speedModifier = iceSpeedMultiplier;
        if (isInWindGust)
            speedModifier *= 0.7f;

        // Beregn endelig bevægelseshastighed
        Vector3 moveVelocity = slidingDirection * speed * speedModifier;

        // Anvend hastighed
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }

    public void StartTorchPickup()
    {
        isPickingUpTorch = true;
        rb.isKinematic = true;
    }

    public void EndTorchPickup()
    {
        isPickingUpTorch = false;
        rb.isKinematic = false;
    }
private bool isRespawning = false;

public void DieAndRespawn()
{
    if (isRespawning) return;

    isRespawning = true;
    StartCoroutine(RespawnCoroutine());
}



    private IEnumerator RespawnCoroutine()
    {

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;


        animator.SetTrigger("Dying");

        // Wait for Dying animation to start
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
            yield return null;

        // Wait for Dying animation to finish
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        // 🔲 Fade to black
        yield return StartCoroutine(FadeBlackOverlay(true));

        // Move to respawn point
        Vector3 respawnPoint = RespawnManager.Instance != null
            ? RespawnManager.Instance.GetRespawnPoint()
            : transform.position;

        transform.position = respawnPoint;

        rb.isKinematic = false;
        rb.constraints &= ~(
            RigidbodyConstraints.FreezePositionX |
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezePositionZ
        );
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        TorchMechanic torch = GetComponentInChildren<TorchMechanic>();
        if (torch != null)
        {
            torch.ReactivateTorchParticles();
        }
        TorchVignette vignette = FindObjectOfType<TorchVignette>();
        if (vignette != null)
        {
            vignette.ResetVignette();
        }



        // 🔲 Fade back in
        yield return StartCoroutine(FadeBlackOverlay(false));
        yield return new WaitForSeconds(1f); // Small delay before trigger


        // ▶️ Play respawn animation now that screen is visible
        animator.SetTrigger("Respawning");

        // Wait for animation to start
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Respawning"))
            yield return null;

        // Wait for animation to finish
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        transform.position = respawnPoint + Vector3.up * 0.5f;

        // rb.velocity = Vector3.zero;
        // rb.angularVelocity = Vector3.zero;
        yield return null;


isRespawning = false;


        Debug.Log("Player respawned.");
        
    }


    private IEnumerator FadeBlackOverlay(bool fadeIn)
    {
        float t = 0f;
        float startAlpha = blackOverlay.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;

        // Make sure it's visible before fading
        blackOverlay.gameObject.SetActive(true);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            blackOverlay.alpha = alpha;
            yield return null;
        }

        blackOverlay.alpha = targetAlpha;

        if (!fadeIn)
        {
            blackOverlay.gameObject.SetActive(false);
        }
    }
    
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Icicle"))
    {
        Debug.Log("Player hit by icicle trigger. Respawning...");
        DieAndRespawn();
    }
}


}
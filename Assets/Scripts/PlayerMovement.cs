using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    CharacterAnimation characterAnimation;

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
    public bool canCrouch = true;
    public float crouchWindResistance = 0.05f;

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
    private bool isCrouching = false;
    private bool isPickingUpTorch = false;
    private bool wasMovingLastFrame = false;
    private Quaternion lastRotationBeforeStop;
    private bool isRespawning = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        characterAnimation = GetComponent<CharacterAnimation>();
        windZone = FindObjectOfType<WindZone>();
        slidingDirection = Vector3.zero;

        if (footstepsAudioSource == null)
        {
            Debug.LogError("Footsteps AudioSource er ikke tildelt!");
            footstepsAudioSource = GetComponent<AudioSource>();
            if (footstepsAudioSource == null)
            {
                footstepsAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        footstepsAudioSource.loop = true;
        footstepsAudioSource.playOnAwake = false;
    }

    void Update()
    {
        // Stop input hvis død eller respawning
        if (!canMove || isRespawning || isDead) return;

        CheckGroundedAndSurface();

        if (windZone != null)
            isInWindGust = windZone.windMain > 12.0f;

        if (!isInWindGust)
            windAnimationTriggered = false;

        // Crouch håndtering
        if (Input.GetKeyDown(KeyCode.LeftShift) && canCrouch && isGrounded)
        {
            isCrouching = true;
            animator.SetBool("IsCrouching", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && canCrouch)
        {
            isCrouching = false;
            animator.SetBool("IsCrouching", false);
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !jumpTriggered && !isCrouching)
        {
            HandleJumping();
        }
    }

    void FixedUpdate()
    {
        // Stop al bevægelse hvis død, respawning eller picking up torch
        if (!canMove || isPickingUpTorch || isRespawning || isDead || rb.isKinematic)
        {
            if (footstepsAudioSource.isPlaying)
                footstepsAudioSource.Stop();
            return;
        }

        float forward = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float sideways = Input.GetKey(KeyCode.S) ? 1 : Input.GetKey(KeyCode.W) ? -1 : 0;

        Vector3 moveDirection = new Vector3(sideways, 0, forward).normalized;
        float actualSpeed = maxMoveSpeed * characterAnimation.velocity;

        // Vindlogik
        if (isInWindGust && windZone != null)
        {
            if (isCrouching && isGrounded)
            {
                // Crouch beskytter mod vind
                float crouchedWindForce = crouchWindResistance;
                Vector3 windDirection = windZone.transform.forward;
                rb.AddForce(windDirection * crouchedWindForce, ForceMode.Force);
                windAnimationTriggered = false;
            }
            else if (isGrounded)
            {
                // Vindkraft kun på jorden - hop håndteres separat
                Vector3 windDirection = windZone.transform.forward;
                float windStrength = windBackwardsForce * 0.7f; // Øget fra 0.6f

                rb.AddForce(windDirection * windStrength, ForceMode.Force);

                if (moveDirection.magnitude < 0.1f && !windAnimationTriggered)
                {
                    animator.SetTrigger("Wind");
                    windAnimationTriggered = true;
                }
            }
            else if (!isGrounded) // I luften under vindstød
            {
                // TILFØJET: MEGET kraftig vindkraft i luften
                Vector3 windDirection = windZone.transform.forward;
                float airborneWindStrength = windBackwardsForce * 1.5f; // Meget kraftig
                rb.AddForce(windDirection * airborneWindStrength, ForceMode.Force);

                Debug.Log("KRAFTIG VINDKRAFT I LUFTEN: " + (windDirection * airborneWindStrength));
            }
        }

        // Normal bevægelse (RETTET: Sluk bevægelse fuldstændigt under vindstød)
        if (!isInWindGust)
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
        // INGEN bevægelse under vindstød - kun vindkraften påvirker ham

        // Fodtrinslyd
        bool hasMovementInput = moveDirection.magnitude > 0.1f;
        bool shouldPlayFootsteps = isGrounded && !jumpTriggered && hasMovementInput && !isOnIce && !isCrouching;

        if (shouldPlayFootsteps)
        {
            if (!footstepsAudioSource.isPlaying)
                footstepsAudioSource.Play();
        }
        else
        {
            if (footstepsAudioSource.isPlaying)
                footstepsAudioSource.Stop();
        }

        // Rotation (RETTET: Ingen rotation under vindstød)
        bool isCurrentlyMoving = moveDirection.magnitude >= 0.1f;
        bool shouldRotate = isCurrentlyMoving && !isInWindGust; // Ingen rotation under vindstød

        if (shouldRotate)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            wasMovingLastFrame = true;
        }
        else
        {
            if (wasMovingLastFrame)
            {
                lastRotationBeforeStop = transform.rotation;
                wasMovingLastFrame = false;
            }
            transform.rotation = lastRotationBeforeStop;
        }
    }

    private void CheckGroundedAndSurface()
    {
        Vector3 spherePosition = transform.position - new Vector3(0, groundCheckDistance / 2, 0);
        float sphereRadius = 0.3f;
        Collider[] hitColliders = Physics.OverlapSphere(spherePosition, sphereRadius, groundLayer);

        isGrounded = hitColliders.Length > 0;
        isOnIce = false;

        if (isGrounded)
        {
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
        FindObjectOfType<AudioManager>().PlayOneShot("Jump");

        Vector3 jumpVector = Vector3.up * jumpForce;

        if (isInWindGust && windZone != null)
        {
            // UNDER VINDSTØD: Ignorer fremad-input og tilføj kraftig vindkraft
            Vector3 windDirection = windZone.transform.forward;
            Vector3 windForce = windDirection * windBackwardsForce * 1.5f;

            // Kombiner hop og vind til ÉN kraft (ingen fremad-input)
            Vector3 combinedForce = jumpVector + windForce;

            rb.AddForce(combinedForce, ForceMode.Impulse);
            Debug.Log("VINDSTØD HOP (ingen fremad): Up=" + jumpVector + " + Wind=" + windForce + " = " + combinedForce);
        }
        else
        {
            // UDEN VINDSTØD: Normal hop med fremad-input
            float forward = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;

            Vector3 forwardMovement = Vector3.zero;
            if (forward != 0)
            {
                forwardMovement = transform.forward * (forward * maxMoveSpeed * forwardJumpForceReduction);
            }

            Vector3 combinedForce = jumpVector + forwardMovement;
            rb.AddForce(combinedForce, ForceMode.Impulse);
            Debug.Log("NORMAL HOP med fremad: " + combinedForce);
        }

        animator.ResetTrigger("JumpTrigger");
        animator.SetTrigger("JumpTrigger");
        jumpTriggered = true;
    }

    private void ApplyNormalMovement(Vector3 moveDirection, float speed)
    {
        if (rb.isKinematic) return;

        if (isCrouching)
            speed *= 0.5f;

        Vector3 moveVelocity = moveDirection * speed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        slidingDirection = Vector3.zero;
    }

    private void ApplyIceMovement(Vector3 moveDirection, float speed)
    {
        if (rb.isKinematic) return;

        if (isCrouching)
            speed *= 0.5f;

        if (moveDirection.magnitude > 0.1f)
        {
            float lerpFactor = 1 - iceSlideFactor;
            slidingDirection = Vector3.Lerp(slidingDirection, moveDirection, lerpFactor);
        }
        else if (slidingDirection.magnitude > 0.01f)
        {
            slidingDirection = Vector3.Lerp(slidingDirection, Vector3.zero, Time.deltaTime * 0.3f);
        }

        float speedModifier = iceSpeedMultiplier;
        if (isInWindGust)
            speedModifier *= 0.7f;

        Vector3 moveVelocity = slidingDirection * speed * speedModifier;
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

    public void DieAndRespawn()
    {
        // Beskyt mod multiple calls
        if (isRespawning || isDead) return;

        Debug.Log("DieAndRespawn called - starting respawn process");
        isRespawning = true;
        isDead = true;
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // Stop al bevægelse og vindpåvirkning ØJEBLIKKELIGT
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Stop alle lyde
        if (footstepsAudioSource.isPlaying)
            footstepsAudioSource.Stop();

        animator.SetTrigger("Dying");

        yield return new WaitForSeconds(2.0f);

        FindObjectOfType<AudioManager>().PlayOneShot("PlayerFall");

        // Vent på dying animation
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        // Fade to black
        yield return StartCoroutine(FadeBlackOverlay(true));

        // Check for end scene
        if (EndSceneTrigger.playerEnteredEndZone)
        {
            Debug.Log("Triggering end cutscene instead of respawning.");
            SceneManager.LoadScene("EndScene");
            yield break;
        }

        // Move to respawn point
        Vector3 respawnPoint = RespawnManager.Instance != null
            ? RespawnManager.Instance.GetRespawnPoint()
            : transform.position;

        transform.position = respawnPoint + Vector3.up * 0.5f;

        // Reset rigidbody korrekt
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset states
        isDead = false;
        isCrouching = false;
        animator.SetBool("IsCrouching", false);

        animator.SetTrigger("Respawning");

        // Vent på respawn animation
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Respawning"))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.1f)
            yield return null;

        // Reaktivér torch
        TorchMechanic torch = GetComponentInChildren<TorchMechanic>();
        if (torch != null)
            torch.ReactivateTorchParticles();

        yield return StartCoroutine(FadeBlackOverlay(false, 0.5f));

        isRespawning = false;
        Debug.Log("Player respawned successfully.");
    }

    private IEnumerator FadeBlackOverlay(bool fadeIn, float duration = 1f)
    {
        float t = 0f;
        float startAlpha = blackOverlay.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;

        if (!blackOverlay.gameObject.activeSelf)
            blackOverlay.gameObject.SetActive(true);

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            blackOverlay.alpha = alpha;
            yield return null;
        }

        blackOverlay.alpha = targetAlpha;

        if (!fadeIn)
            blackOverlay.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Beskyt mod multiple trigger calls
        if (isRespawning || isDead) return;

        if (other.CompareTag("Icicle"))
        {
            Debug.Log("Player hit by icicle trigger. Respawning...");
            DieAndRespawn();
        }
    }
}
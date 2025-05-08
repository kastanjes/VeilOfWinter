using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    CharacterAnimation characterAnimation;

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

    [Header("Ice Settings")]
    public float iceSpeedMultiplier = 1.8f;
    public float iceSlideFactor = 0.95f;
    public string iceSurfaceTag = "Ice";

    private bool isGrounded;
    private bool jumpTriggered = false;
    private bool isInWindGust = false;
    private bool isOnIce = false;
    private bool windAnimationTriggered = false;
    private WindZone windZone;
    private Vector3 slidingDirection;

    private bool isPickingUpTorch = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        characterAnimation = GetComponent<CharacterAnimation>();
        
        windZone = FindObjectOfType<WindZone>();
        slidingDirection = Vector3.zero;
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

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !jumpTriggered)
        {
            HandleJumping();
        }
    }

    void FixedUpdate()
    {
        if (!canMove || isPickingUpTorch) return;

        float forward = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float sideways = Input.GetKey(KeyCode.S) ? 1 : Input.GetKey(KeyCode.W) ? -1 : 0;

        Vector3 moveDirection = new Vector3(sideways, 0, forward).normalized;
        float actualSpeed = maxMoveSpeed * characterAnimation.velocity;
        
        if (isInWindGust && windZone != null)
        {
            float windMultiplier = isGrounded ? 2f : 4f;
            rb.AddForce(windZone.transform.forward * -windMultiplier, ForceMode.Force);

            if (moveDirection.magnitude < 0.1f && isGrounded && !windAnimationTriggered)
            {
                animator.SetTrigger("Wind");
                windAnimationTriggered = true;
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

if (moveDirection.z != 0)
{
    Vector3 scale = transform.localScale;
    scale.x = moveDirection.z > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
    transform.localScale = scale;
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
        Vector3 moveVelocity = moveDirection * speed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        slidingDirection = Vector3.zero;
    }

    private void ApplyIceMovement(Vector3 moveDirection, float speed)
    {
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
}
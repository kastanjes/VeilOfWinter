using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    Animator animator;

    public float maxMoveSpeed = 5f;
    public float jumpForce = 7f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpTriggered = false;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;
    private CharacterAnimation characterAnimation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        characterAnimation = GetComponent<CharacterAnimation>();
    }

    void Update()
    {
        // Ground check using raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        // Reset jump if player is grounded again
        if (isGrounded)
        {
            jumpTriggered = false;
        }
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !jumpTriggered)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpTriggered = true;

            // 🚀 TRIGGER jump animation
            animator.ResetTrigger("JumpTrigger"); // optional safety
            animator.SetTrigger("JumpTrigger");
        }
    }

    void FixedUpdate()
    {
        float forward = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float sideways = Input.GetKey(KeyCode.S) ? 1 : Input.GetKey(KeyCode.W) ? -1 : 0;

        Vector3 moveDirection = new Vector3(sideways, 0, forward).normalized;
        float actualSpeed = maxMoveSpeed * characterAnimation.velocity;

        Vector3 moveVelocity = moveDirection * actualSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}

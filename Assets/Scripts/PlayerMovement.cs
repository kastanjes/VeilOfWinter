using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Move forward (D) and backward (A) on the Z-axis
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, moveInput * moveSpeed);

        // Check if player is grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }
}
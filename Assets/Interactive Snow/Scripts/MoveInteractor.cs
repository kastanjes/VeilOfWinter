using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInteractor : MonoBehaviour
{

    Vector3 moveDirection;
    float horizontal;
    float vertical;
    Rigidbody rb;
    [SerializeField]
    float moveSpeed;
    bool grounded;

    [SerializeField] LayerMask layerMask;
    [SerializeField] float distance = 0.1f;

    float moveAmount;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * distance, grounded ? Color.green : Color.red);

    }

    void GetMovement()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        Vector3 v = vertical * Camera.main.transform.forward;
        Vector3 h = horizontal * Camera.main.transform.right;

        v.y = 0f;
        h.y = 0f;

        float moveAm = Mathf.Abs(horizontal) + Mathf.Abs(vertical);


        moveDirection = new Vector3((v + h).normalized.x, 0, (v + h).normalized.z);

        moveAmount = Mathf.Clamp01(moveAm);
    }


    private void FixedUpdate()
    {
        rb.AddRelativeTorque(moveDirection);
        GetMovement();
        RotateToDir();
        grounded = CheckGround();

        rb.velocity = (moveDirection * moveSpeed * moveAmount);
        if (!grounded)
        {
            rb.velocity += Physics.gravity * Time.fixedDeltaTime * 20;
        }

    }

    void RotateToDir()
    {
        Vector3 targetDir = moveDirection;

        targetDir.y = 0;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }
        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Lerp(transform.rotation, tr, Time.deltaTime * moveAmount * 10);


        transform.rotation = targetRotation * transform.rotation;
    }

    bool CheckGround()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, distance, layerMask))
        {
            // Align to the ground normal
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            return true;
        }

        return false;
    }
}

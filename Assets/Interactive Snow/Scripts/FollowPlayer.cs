using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float distance = 5f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float minVerticalAngle = -30f;
    [SerializeField] float maxVerticalAngle = 60f;

    private float currentRotationX = 0f;

    GameObject Dummy;

    void Start()
    {
        // Initialize camera position and rotation
        Dummy = new GameObject();
        Dummy.transform.position = target.position;
        transform.position = target.position - Vector3.forward * distance;
        transform.LookAt(Dummy.transform);

    }

    void Update()
    {
        Dummy.transform.position = target.position;
        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Rotate the camera horizontally around the character
        Dummy.transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically around the character
        currentRotationX -= mouseY;
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

        transform.rotation = Quaternion.Euler(currentRotationX, Dummy.transform.eulerAngles.y, 0);

        // Update camera position
        transform.position = Dummy.transform.position - transform.forward * distance;
    }
}

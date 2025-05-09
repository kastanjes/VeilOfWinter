using UnityEngine;

public class MenuParallaxCamera : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float maxOffset = 0.5f;
    public float smoothing = 5f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        // Mouse position normalized from (0 to 1)
        Vector2 mousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        // Convert to offset range (-1 to 1)
        Vector2 offset = (mousePos - new Vector2(0.5f, 0.5f)) * 2f;

        // Apply offset: X stays X, Y becomes Z
        Vector3 targetPosition = initialPosition + new Vector3(offset.x * maxOffset, 0f, offset.y * maxOffset);

        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);
    }
}

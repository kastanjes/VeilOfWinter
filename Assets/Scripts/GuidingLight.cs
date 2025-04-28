using System.Collections.Generic;
using UnityEngine;

public class GuidingLightPath : MonoBehaviour
{
    public List<Transform> waypoints;   // Assign your waypoints in the Inspector
    public float moveSpeed = 2f;         // Movement speed
    public float hoverAmplitude = 0.5f;  // How much it floats
    public float hoverFrequency = 2f;    // How fast it floats

    private int currentWaypoint = 0;
    private bool moving = false;
    private Vector3 startPosition;

    void Start()
    {
        if (waypoints.Count > 0)
            transform.position = waypoints[0].position;
        startPosition = transform.position;
    }

    void Update()
    {
        if (moving && currentWaypoint < waypoints.Count)
        {
            Vector3 targetPosition = waypoints[currentWaypoint].position;

            // Add hover animation
            float hover = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            Vector3 hoverOffset = new Vector3(0, hover, 0);

            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Apply hover
            transform.position += hoverOffset * Time.deltaTime;

            // If close enough to the waypoint
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                moving = false; // Wait until triggered again
            }
        }
    }

    public void MoveToNextWaypoint()
    {
        if (currentWaypoint < waypoints.Count - 1)
        {
            currentWaypoint++;
            moving = true;
        }
    }
}

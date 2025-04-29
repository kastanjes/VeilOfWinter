using System.Collections.Generic;
using UnityEngine;

public class GuidingLightPath : MonoBehaviour
{
    public List<Transform> waypoints;
    public float moveSpeed = 2f;
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 2f;

    private int currentWaypoint = 0;
    private bool moving = false;
    private Vector3 startPosition;
    private Vector3 hoverCenter;

    void Start()
    {
        if (waypoints.Count > 0)
        {
            transform.position = waypoints[0].position;
            hoverCenter = transform.position; // Save the center for hovering
        }
    }

    void Update()
    {
        if (waypoints.Count == 0)
            return;

        // Always hover, moving or not
        float hover = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        Vector3 hoverOffset = new Vector3(0, hover, 0);

        if (moving && currentWaypoint < waypoints.Count)
        {
            Vector3 targetPosition = waypoints[currentWaypoint].position;
            Vector3 moveDirection = (targetPosition - hoverCenter).normalized;

            hoverCenter += moveDirection * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(hoverCenter, targetPosition) < 0.5f)
            {
                moving = false;
                hoverCenter = targetPosition; // Snap to the waypoint center
            }
        }

        transform.position = hoverCenter + hoverOffset;
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

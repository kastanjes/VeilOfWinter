using UnityEngine;

public class WaypointTrigger : MonoBehaviour
{
    public GuidingLightPath guidingLight;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            guidingLight.MoveToNextWaypoint();
            Destroy(gameObject); // Remove trigger so it only activates once
        }
    }
}

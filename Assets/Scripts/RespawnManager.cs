using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }
    
    [SerializeField] private Vector3 defaultRespawnPosition = Vector3.zero; // Set this i inspector
    private Vector3 lastRespawnPosition;
    private bool hasRespawnPoint = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Set initial respawn point til spillerens start position
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                defaultRespawnPosition = player.transform.position;
                lastRespawnPosition = defaultRespawnPosition;
                hasRespawnPoint = true;
                Debug.Log($"Initial respawn point set to: {defaultRespawnPosition}");
            }
            else
            {
                Debug.LogWarning("No player found with 'Player' tag. Using default respawn position.");
            }
        }
        else
            Destroy(gameObject);
    }

    public void SetRespawnPoint(Vector3 newPosition)
    {
        lastRespawnPosition = newPosition;
        hasRespawnPoint = true;
        Debug.Log($"New respawn point set: {newPosition}");
    }

    public Vector3 GetRespawnPoint()
    {
        Vector3 point = hasRespawnPoint ? lastRespawnPosition : defaultRespawnPosition;
        Debug.Log($"Getting respawn point: {point}");
        return point;
    }

    public bool HasRespawnPoint()
    {
        return hasRespawnPoint;
    }
}
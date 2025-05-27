using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private Vector3 lastRespawnPosition;
    private bool hasRespawnPoint = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetRespawnPoint(Vector3 newPosition)
    {
        lastRespawnPosition = newPosition;
        hasRespawnPoint = true;
    }

    public Vector3 GetRespawnPoint()
    {
        return lastRespawnPosition;
    }

    public bool HasRespawnPoint()
    {
        return hasRespawnPoint;
    }
}


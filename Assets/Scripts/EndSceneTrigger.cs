using UnityEngine;

public class EndSceneTrigger : MonoBehaviour
{
    public static bool playerEnteredEndZone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerEnteredEndZone = true;
            Debug.Log("Player entered end cutscene zone.");
        }
    }

    private void OnTriggerStay(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Debug.Log("Player is staying inside the end zone trigger.");
    }
}

}

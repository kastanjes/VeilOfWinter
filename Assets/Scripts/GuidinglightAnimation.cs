using UnityEngine;
using System.Collections;

public class GuidingLightController : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    public float waitAtEachPoint = 0f;

    private int currentIndex = 0;
    private bool moving = false;
    private int torchesActivated = 0; // Tæller for aktiverede torches

    void Start()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger with: " + other.name);

        if (other.CompareTag("CollectableTorch"))
        {
            var particles = other.GetComponentInChildren<ParticleSystem>();
            if (particles != null)
            {
                particles.gameObject.SetActive(true);
                particles.Play();
                
                // 🔊 AFSPIL LYDEFFEKT KUN PÅ DE FØRSTE TO TORCHES
                if (torchesActivated < 2)
                {
                    FindObjectOfType<AudioManager>().PlayOneShot("LightTorch");
                    Debug.Log($"Played sound for torch #{torchesActivated + 1}: " + other.name);
                }
                else
                {
                    Debug.Log($"No sound for torch #{torchesActivated + 1} (beyond first 2): " + other.name);
                }
                
                torchesActivated++; // Øg tælleren
                
                Debug.Log("Activated particles for: " + other.name);
            }
            else
            {
                Debug.LogWarning("No ParticleSystem found on: " + other.name);
            }
        }
    }

    public IEnumerator MoveToNextWaypoint()
    {
        moving = true;

        while (currentIndex < waypoints.Length)
        {
            Transform target = waypoints[currentIndex];

            while (Vector3.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                yield return null;
            }

            currentIndex++;
            yield return new WaitForSeconds(waitAtEachPoint);
        }

        moving = false;

        // Optional: destroy or fade out after final point
        Destroy(gameObject, 1f);
    }
}
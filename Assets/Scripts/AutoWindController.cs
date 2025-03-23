using System.Collections;
using UnityEngine;

public class AutoWindController : MonoBehaviour
{
    [Header("Gameplay Settings")]
    [Tooltip("Hvor kraftig vinden er under et vindstød")]
    public float windStormForce = 20f;
    
    [Tooltip("Basis vindstyrke (når der ikke er vindstød)")]
    public float baseWindForce = 0.3f;
    
    [Tooltip("Hvor lang tid der går mellem vindstød (min)")]
    public float minTimeBetweenGusts = 3f;
    
    [Tooltip("Hvor lang tid der går mellem vindstød (max)")]
    public float maxTimeBetweenGusts = 8f;
    
    [Tooltip("Hvor længe et vindstød varer (min)")]
    public float minGustDuration = 1.5f;
    
    [Tooltip("Hvor længe et vindstød varer (max)")]
    public float maxGustDuration = 3.5f;
    
    [Header("Jump Detection")]
    [Tooltip("Tag på spillerobjektet")]
    public string playerTag = "Player";
    
    [Tooltip("Hvor højt spilleren skal være over jorden for at vinden rammer hårdere")]
    public float airborneHeight = 0.5f;
    
    [Tooltip("Multiplikator for vindstyrke når spilleren er i luften")]
    public float airborneWindMultiplier = 10f;
    
    // Private variabler
    private bool isGustActive = false;
    private float nextGustTime;
    private GameObject playerObject;
    private Rigidbody playerRigidbody;
    private float lastGroundedY;
    private bool wasAirborne = false;
    private WindZone windZone;
    
    void Start()
    {
        Debug.Log("AutoWindController startet");
        
        // Find WindZone automatisk (enten på samme objekt eller i scenen)
        windZone = GetComponent<WindZone>();
        if (windZone == null)
        {
            windZone = FindObjectOfType<WindZone>();
            if (windZone == null)
            {
                Debug.LogWarning("Ingen WindZone fundet - tilføjer en ny");
                windZone = gameObject.AddComponent<WindZone>();
            }
        }
        
        // Konfigurer WindZone
        windZone.mode = WindZoneMode.Directional;
        windZone.windMain = baseWindForce;
        
        Debug.Log("WindZone fundet/oprettet: " + windZone.name);
        
        // Find spilleren automatisk
        playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerRigidbody = playerObject.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                lastGroundedY = playerObject.transform.position.y;
                Debug.Log("Spiller fundet: " + playerObject.name);
            }
            else
            {
                Debug.LogWarning("Spiller har ikke en Rigidbody komponent");
            }
        }
        else
        {
            Debug.LogWarning("Ingen spiller med tag '" + playerTag + "' fundet");
        }
        
        // Start vindcyklus
        ScheduleNextGust();
    }
    
    void FixedUpdate()
    {
        // Håndter vindstød timing
        HandleWindGustTiming();
        
        // Tjek om spilleren er i luften
        if (playerObject != null && playerRigidbody != null)
        {
            CheckPlayerAirborne();
        }
    }
    
    private void HandleWindGustTiming()
    {
        // Start et vindstød hvis tiden er inde
        if (!isGustActive && Time.time >= nextGustTime)
        {
            StartCoroutine(TriggerWindGust());
        }
    }
    
    private void CheckPlayerAirborne()
    {
        // Beregn om spilleren er i luften
        bool isAirborne = (playerObject.transform.position.y - lastGroundedY) > airborneHeight;
        
        // Debug log for at se om spilleren er i luften
        if (isAirborne != wasAirborne)
        {
            Debug.Log("Spiller airborne status ændret: " + isAirborne);
        }
        
        // Opdater lastGroundedY hvis spilleren er på jorden
        if (!isAirborne)
        {
            lastGroundedY = playerObject.transform.position.y;
        }
        
        // Hvis spilleren er i luften under et vindstød
        if (isAirborne && isGustActive)
        {
            // Beregn vindkraften
            Vector3 windForce = windZone.transform.forward * windStormForce * (isAirborne ? airborneWindMultiplier : 1f);
            
            // Tilføj kraft til spilleren
            playerRigidbody.AddForce(windForce * Time.deltaTime, ForceMode.Force);
            
            // Debug info
            Debug.Log("Tilføjer vindkraft: " + windForce + " til spiller: " + playerObject.name);
        }
        
        // Opdater airborne status
        wasAirborne = isAirborne;
    }
    
    private IEnumerator TriggerWindGust()
    {
        Debug.Log("Vindstød starter med styrke: " + windStormForce);
        isGustActive = true;
        
        // Fuld vindstyrke
        windZone.windMain = windStormForce;
        
        // Beregn hvor længe vindstødet skal vare
        float gustDuration = Random.Range(minGustDuration, maxGustDuration);
        Debug.Log("Vindstød varer: " + gustDuration + " sekunder");
        yield return new WaitForSeconds(gustDuration);
        
        // Normal vindstyrke
        windZone.windMain = baseWindForce;
        Debug.Log("Vindstød slut");
        
        // Reset flag og planlæg næste vindstød
        isGustActive = false;
        ScheduleNextGust();
    }
    
    private void ScheduleNextGust()
    {
        nextGustTime = Time.time + Random.Range(minTimeBetweenGusts, maxTimeBetweenGusts);
        Debug.Log("Næste vindstød planlagt til: " + (nextGustTime - Time.time) + " sekunder fra nu");
    }
    
    // Metode til at manuelt trigge et vindstød (kan kaldes fra inspektor eller andre scripts)
    public void TriggerGustManually()
    {
        if (!isGustActive)
        {
            Debug.Log("Manuelt vindstød aktiveret");
            StartCoroutine(TriggerWindGust());
        }
    }
    
    // Visualisering af vindretning i editoren
    void OnDrawGizmos()
    {
        // Hvis vi allerede har en WindZone, brug dens retning
        WindZone visibleZone = GetComponent<WindZone>();
        if (visibleZone == null) visibleZone = FindObjectOfType<WindZone>();
        
        if (visibleZone != null)
        {
            Gizmos.color = isGustActive ? Color.red : Color.blue;
            Gizmos.DrawRay(transform.position, visibleZone.transform.forward * 5f);
        }
    }
    
    // Tilføj en knap i inspektoren til manuel test
    void OnValidate()
    {
        // Sikrer at WindZone komponenten eksisterer på samme GameObject
        WindZone existingZone = GetComponent<WindZone>();
        if (existingZone == null)
        {
            Debug.Log("WindZone mangler - tilføjer automatisk");
            gameObject.AddComponent<WindZone>();
        }
    }
}
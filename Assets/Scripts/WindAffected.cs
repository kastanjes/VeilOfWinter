using UnityEngine;

public class OptimizedWindAffected : MonoBehaviour
{
    public float windMultiplier = 1.0f;
    
    // Caching references
    private Rigidbody rb;
    private static WindZone cachedWindZone;
    
    // Tidsstyret opdatering
    private float nextPhysicsUpdate = 0f;
    public float physicsUpdateRate = 0.1f; // Opdater vindkraften hver 0.1 sekund i stedet for hver frame
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Kun find WindZone én gang for alle instanser af denne klasse
        if (cachedWindZone == null)
        {
            cachedWindZone = FindObjectOfType<WindZone>();
        }
        
        // Tilføj et lille forsinkelse, så ikke alle objekter opdateres samtidigt
        nextPhysicsUpdate = Time.time + Random.Range(0f, physicsUpdateRate);
    } 
    
    void FixedUpdate()
    {
        // Tidlig returnering hvis vi mangler nødvendige komponenter
        if (cachedWindZone == null || rb == null) return;
        
        // Kun opdater kraft med bestemte intervaller
        if (Time.time >= nextPhysicsUpdate)
        {
            // Beregn vindkraft
            Vector3 windDirection = cachedWindZone.transform.forward;
            float windStrength = cachedWindZone.windMain;
            
            // Tilføj kraft
            rb.AddForce(windDirection * windStrength * windMultiplier, ForceMode.Acceleration);
            
            // Planlæg næste opdatering
            nextPhysicsUpdate = Time.time + physicsUpdateRate;
        }
    }
}
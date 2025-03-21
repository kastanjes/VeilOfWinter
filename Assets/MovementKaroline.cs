using UnityEngine;

public class MovementKaroline : MonoBehaviour
{
    public float moveSpeed = 5f;    
    public float jumpForce = 7f;
    public float windBackwardsForce = 15f; // Kraft spilleren skubbes baglæns med under vindstød
    public float forwardJumpForceReduction = 0.7f; // Reduktion af fremadrettet kraft under vindstød (0-1)

    private Rigidbody rb;
    private bool isGrounded;
    private float moveInput;
    private bool isInWindGust = false;
    private WindZone windZone;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("Movement script startet på: " + gameObject.name);
        
        // Find WindZone i scenen
        windZone = FindObjectOfType<WindZone>();
    }

    void Update()
    {
        // Check if player is grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // Gem input til FixedUpdate
        moveInput = Input.GetAxisRaw("Horizontal"); 

        // Tjek om der er et kraftigt vindstød ved at overvåge WindZone
        if (windZone != null)
        {
            // Hvis vindstyrken er over 10, er det et kraftigt vindstød
            isInWindGust = windZone.windMain > 10.0f;
        }

        // Jumping - håndterer nu retningsbestemt input under hop
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Basis-hopvektor (opad)
            Vector3 jumpVector = Vector3.up * jumpForce;
            
            // Hvis der er et vindstød aktivt
            if (isInWindGust)
            {
                Vector3 windDirection = windZone.transform.forward;
                Vector3 forwardMovement = Vector3.zero;
                
                // Hvis spilleren prøver at bevæge sig fremad (mod vinden)
                if (moveInput > 0)
                {
                    // Reducerer fremadrettet bevægelseskraft betydeligt
                    forwardMovement = transform.forward * (moveInput * moveSpeed * forwardJumpForceReduction);
                    
                    // Tilføjer en kraftig baglæns-komponent (modsat vindretningen)
                    Vector3 backwardsVector = -windDirection * windBackwardsForce;
                    
                    // Kombineret hop: op + reduceret fremad + kraftig baglæns
                    rb.AddForce(jumpVector + forwardMovement + backwardsVector, ForceMode.Impulse);
                    
                    Debug.Log("Spilleren hopper FREMAD under vindstød, men bliver skubbet BAGLÆNS!");
                }
                // Hvis spilleren prøver at bevæge sig i andre retninger eller står stille
                else
                {
                    // Baglæns-komponent (modsat vindretningen)
                    Vector3 backwardsVector = -windDirection * windBackwardsForce;
                    
                    // Hvis spilleren hopper baglæns eller neutral
                    if (moveInput < 0)
                    {
                        // Tilføj ekstra baglæns-kraft hvis spilleren allerede bevæger sig baglæns
                        backwardsVector *= 1.2f;
                        Debug.Log("Spilleren hopper BAGLÆNS under vindstød - ekstra baglæns kraft!");
                    }
                    else
                    {
                        Debug.Log("Spilleren hopper NEUTRALT under vindstød - bliver skubbet baglæns!");
                    }
                    
                    // Kombineret hop: op + baglæns
                    rb.AddForce(jumpVector + backwardsVector, ForceMode.Impulse);
                }
            }
            else
            {
                // Normalt hop uden vindstød
                // Tilføj en fremadrettet komponent, hvis spilleren trykker en retning
                Vector3 directionVector = transform.forward * (moveInput * moveSpeed * 0.5f);
                
                // Kombineret hop: op + retning
                rb.AddForce(jumpVector + directionVector, ForceMode.Impulse);
                Debug.Log("Spilleren hopper normalt med retning: " + moveInput);
            }
        }
    }

    void FixedUpdate()
    {
        // Hvis spilleren er på jorden, håndter horisontal bevægelse her
        if (isGrounded)
        {
            // Bevar kontrol over spillerens horisontale bevægelse, men tilføj modstand under vindstød
            float currentSpeedMultiplier = isInWindGust ? 0.7f : 1.0f;
            
            // Anvendt bevægelse baseret på input
            rb.velocity = new Vector3(
                rb.velocity.x, 
                rb.velocity.y, 
                moveInput * moveSpeed * currentSpeedMultiplier
            );
        }
        else
        {
            // I luften - bevar eksisterende horisontal hastighed, vind påvirker gennem AddForce
            rb.velocity = new Vector3(
                rb.velocity.x,
                rb.velocity.y,
                rb.velocity.z
            );
        }
        
        // Tilføj konstant lille kraft mod spilleren under vindstød
        if (isInWindGust && windZone != null)
        {
            // Større vindmodstand i luften
            float windMultiplier = isGrounded ? 2f : 4f;
            rb.AddForce(windZone.transform.forward * -windMultiplier, ForceMode.Force);
        }
    }

    // Dette viser spillerens status når der er et vindstød
    void OnGUI()
    {
        if (isInWindGust)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, 10, 200, 20), "VINDSTØD AKTIV!");
        }
    }
}
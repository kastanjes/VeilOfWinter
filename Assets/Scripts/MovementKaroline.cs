using UnityEngine;

public class MovementKaroline : MonoBehaviour
{
    [Header("Bevægelse og Hop")]
    public float moveSpeed = 5f;    
    public float jumpForce = 7f;
    
    [Header("Vind Indstillinger")]
    public float windBackwardsForce = 15f; // Kraft spilleren skubbes baglæns med under vindstød
    public float forwardJumpForceReduction = 0.7f; // Reduktion af fremadrettet kraft under vindstød (0-1)
    
    [Header("Is Indstillinger")]
    public float iceSpeedMultiplier = 1.8f; // Hvor meget hurtigere spilleren er på is
    public float iceSlideFactor = 0.95f;    // Hvor meget momentum der bevares (0-1)
    public string iceSurfaceTag = "Ice";    // Tag til at identificere is-overflader

    private Rigidbody rb;
    private bool isGrounded;
    private float moveInput;
    private bool isInWindGust = false;
    private bool isOnIce = false;
    private WindZone windZone;
    private Vector3 slidingDirection; // Retning for glidning på is

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("Movement script startet på: " + gameObject.name);
        
        // Find WindZone i scenen
        windZone = FindObjectOfType<WindZone>();
        
        // Initialiser slidingDirection
        slidingDirection = Vector3.zero;
    }

    void Update()
    {
        // Check if player is grounded and on ice
        CheckGroundedAndSurface();

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
            HandleJumping();
        }
    }

    void FixedUpdate()
    {
        // Håndter bevægelse baseret på overflade
        if (isOnIce)
        {
            ApplyIceMovement();
        }
        else
        {
            ApplyNormalMovement();
        }
        
        // Tilføj konstant lille kraft mod spilleren under vindstød
        // Dette påvirker både normal grund og is
        if (isInWindGust && windZone != null)
        {
            // Større vindmodstand i luften
            float windMultiplier = isGrounded ? 2f : 4f;
            rb.AddForce(windZone.transform.forward * -windMultiplier, ForceMode.Force);
        }
    }
    
    private void CheckGroundedAndSurface()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f);
        
        if (isGrounded)
        {
            // Tjek om vi står på is baseret på tag
            isOnIce = hit.collider.CompareTag(iceSurfaceTag);
        }
        else
        {
            // Vi er ikke på is når vi er i luften
            isOnIce = false;
        }
    }
    
    private void HandleJumping()
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
            
            // På is, tilføj lidt af den aktuelle glideretning til hoppet
            if (isOnIce && slidingDirection.magnitude > 0.1f)
            {
                directionVector += slidingDirection * moveSpeed * 0.3f;
            }
            
            // Kombineret hop: op + retning
            rb.AddForce(jumpVector + directionVector, ForceMode.Impulse);
            Debug.Log("Spilleren hopper normalt med retning: " + moveInput);
        }
    }
    
    private void ApplyNormalMovement()
    {
        // Normal bevægelseslogik - den samme som i dit originale script
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
            
            // Reset slidingDirection når vi er på normal grund
            slidingDirection = Vector3.zero;
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
    }
    
    private void ApplyIceMovement()
    {
        // Kun anvendt når vi er på is
        if (isGrounded)
        {
            // Beregn målretning baseret på input
            Vector3 targetDirection = new Vector3(0, 0, moveInput).normalized;
            
            // Hvis der er input, opdater slidingDirection
            if (moveInput != 0)
            {
                // Blød overgang fra nuværende retning til ny retning
                slidingDirection = Vector3.Lerp(
                    slidingDirection,
                    targetDirection,
                    1 - iceSlideFactor // Lavere = mere glat
                );
            }
            
            // Beregn hastighed med is-multiplikator og vindmodstand
            float speedModifier = iceSpeedMultiplier;
            if (isInWindGust) speedModifier *= 0.7f; // Samme vindmodstand som på normal grund
            
            // Anvend bevægelse med bevaret momentum
            rb.velocity = new Vector3(
                rb.velocity.x,
                rb.velocity.y,
                slidingDirection.z * moveSpeed * speedModifier
            );
        }
    }

    // Dette viser spillerens status når der er et vindstød eller på is
    void OnGUI()
    {
        if (isInWindGust)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, 10, 200, 20), "VINDSTØD AKTIV!");
        }
        
        if (isOnIce)
        {
            GUI.color = Color.cyan;
            GUI.Label(new Rect(10, 30, 200, 20), "PÅ IS - GLIDER!");
        }
    }
}
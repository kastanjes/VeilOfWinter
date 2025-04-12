using UnityEngine;
using System.Collections;

public class ThinIce : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material crackedMaterial;
    [SerializeField] private GameObject crackEffect; // Valgfri partikeleffekt

    [Header("Timing")]
    [SerializeField] private float crackDelay = 0.5f;
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private float resetDelay = 3f;
    
    private bool isActive = true;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer && normalMaterial)
            meshRenderer.material = normalMaterial;
            
        if (crackEffect)
            crackEffect.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            StartCoroutine(ActivateIceTrap(other.gameObject));
        }
    }
    
    private IEnumerator ActivateIceTrap(GameObject player)
    {
        isActive = false;
        
        // Ændre udseende til revnet is
        if (meshRenderer && crackedMaterial)
            meshRenderer.material = crackedMaterial;
            
        // Aktiver revne-effekt hvis den findes
        if (crackEffect)
            crackEffect.SetActive(true);
            
        // Afspil lyd hvis tilgængelig
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource)
            audioSource.Play();
        
        // Vent kort tid før spiller falder igennem
        yield return new WaitForSeconds(crackDelay);
        
        // Gem spillerens oprindelige position
        Vector3 startPos = player.transform.position;
        Vector3 targetPos = new Vector3(startPos.x, startPos.y - 5f, startPos.z); // Juster -5f efter behov
        
        // Bevæg spilleren gradvist igennem platformen
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            player.transform.position = Vector3.Lerp(startPos, targetPos, elapsed/fallDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Sikrer at spilleren når målpositionen
        player.transform.position = targetPos;
        
        // Vent før is-fælden nulstilles
        yield return new WaitForSeconds(resetDelay);
        
        // Genaktiver fælden og normaliser udseendet
        if (meshRenderer && normalMaterial)
            meshRenderer.material = normalMaterial;
            
        if (crackEffect)
            crackEffect.SetActive(false);
            
        isActive = true;
    }
}
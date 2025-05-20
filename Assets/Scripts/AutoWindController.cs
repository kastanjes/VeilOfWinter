using System.Collections;
using UnityEngine;

public class AutoWindController : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float windStormForce = 20f;
    public float baseWindForce = 3f;
    public float maxTimeBetweenGusts = 8f;
    public float minTimeBetweenGusts = 3f;
    public float minGustDuration = 1.5f;
    public float maxGustDuration = 3.5f;

    [Header("Jump Detection")]
    public string playerTag = "Player";
    public float airborneHeight = 0.5f;
    public float airborneWindMultiplier = 10f;

    [Header("Visual Wind FX")]
    public WindVisualController windVisual;

    private bool isGustActive = false;
    private float nextGustTime;
    private GameObject playerObject;
    private Rigidbody playerRigidbody;
    private float lastGroundedY;
    private bool wasAirborne = false;
    private WindZone windZone;

    void Start()
    {
        windZone = GetComponent<WindZone>() ?? FindObjectOfType<WindZone>();
        if (windZone == null)
        {
            windZone = gameObject.AddComponent<WindZone>();
        }

        windZone.mode = WindZoneMode.Directional;
        windZone.windMain = baseWindForce;

        playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerRigidbody = playerObject.GetComponent<Rigidbody>();
            lastGroundedY = playerObject.transform.position.y;
        }

        ScheduleNextGust();
    }

    void FixedUpdate()
    {
        HandleWindGustTiming();
    }

    private void HandleWindGustTiming()
    {
        if (playerObject != null && playerRigidbody != null)
            CheckPlayerAirborne();

        if (!isGustActive && windVisual != null)
        {
            if (Time.time >= nextGustTime)
                StartCoroutine(TriggerWindGust());
        }
    }

    private void CheckPlayerAirborne()
    {
        bool isAirborne = (playerObject.transform.position.y - lastGroundedY) > airborneHeight;

        if (!isAirborne)
        {
            lastGroundedY = playerObject.transform.position.y;
        }

        if (isAirborne && isGustActive)
        {
            Vector3 windForce = windZone.transform.forward * windStormForce * airborneWindMultiplier;
            playerRigidbody.AddForce(windForce * Time.deltaTime, ForceMode.Force);
        }

        wasAirborne = isAirborne;
    }

    private IEnumerator TriggerWindGust()
    {
        isGustActive = true;
        windZone.windMain = windStormForce;

        if (windVisual != null)
        {
            Vector3 windDir = windZone.transform.forward;
            windVisual.SetParticleWindDirection(windDir, speed: 10f);
            windVisual.PositionFX(playerObject.transform.position, windDir, 2.5f, 1.5f);
            windVisual.ShowWind();
        }

        float gustDuration = Random.Range(minGustDuration, maxGustDuration);
        yield return new WaitForSeconds(gustDuration);

        windZone.windMain = baseWindForce;

        if (windVisual != null)
            windVisual.HideWind();

        isGustActive = false;
        ScheduleNextGust();
    }

    private void ScheduleNextGust()
    {
        nextGustTime = Time.time + Random.Range(minTimeBetweenGusts, maxTimeBetweenGusts);
    }

    public void TriggerGustManually()
    {
        if (!isGustActive)
            StartCoroutine(TriggerWindGust());
    }

    void OnDrawGizmos()
    {
        var zone = GetComponent<WindZone>() ?? FindObjectOfType<WindZone>();
        if (zone != null)
        {
            Gizmos.color = isGustActive ? Color.red : Color.cyan;
            Gizmos.DrawRay(transform.position, zone.transform.forward * 5f);
        }
    }

    void OnValidate()
    {
        if (GetComponent<WindZone>() == null)
            gameObject.AddComponent<WindZone>();
    }
}
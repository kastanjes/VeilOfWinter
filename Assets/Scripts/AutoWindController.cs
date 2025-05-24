// Assets/Scripts/AutoWindController.cs

using System.Collections;
using UnityEngine;

public class AutoWindController : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float windStormForce = 20f;
    public float baseWindForce = 3f;
    public float minTimeBetweenGusts = 3f;
    public float maxTimeBetweenGusts = 8f;
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
    private Animator playerAnimator;

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
            playerAnimator = playerObject.GetComponent<Animator>();
            lastGroundedY = playerObject.transform.position.y;
        }
        else
        {
            Debug.LogWarning($"No GameObject with tag '{playerTag}' found.");
        }

        ScheduleNextGust();
    }

    void FixedUpdate()
    {
        HandleWindGustTiming();

        if (playerObject != null && playerRigidbody != null)
            CheckPlayerAirborne();

        if (isGustActive && windVisual != null)
            PositionWindFX();
    }

    private void HandleWindGustTiming()
    {
        if (!isGustActive && Time.time >= nextGustTime)
            StartCoroutine(TriggerWindGust());
    }

    private void CheckPlayerAirborne()
    {
        bool isAirborne = (playerObject.transform.position.y - lastGroundedY) > airborneHeight;

        if (!isAirborne)
            lastGroundedY = playerObject.transform.position.y;

        if (isAirborne && !wasAirborne)
        {
            Vector3 windForce = windZone.transform.forward * windStormForce * airborneWindMultiplier;
            playerRigidbody.AddForce(windForce * Time.deltaTime, ForceMode.Force);

            if (playerAnimator != null)
                playerAnimator.SetTrigger("Wind");
        }

        wasAirborne = isAirborne;
    }

    private IEnumerator TriggerWindGust()
    {
        isGustActive = true;
        windZone.windMain = windStormForce;

        if (windVisual != null)
        {
            SetParticleWindDirection();
            PositionWindFX();
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

    private void SetParticleWindDirection()
    {
        if (windVisual?.windParticles == null || windZone == null)
            return;

        var velocityModule = windVisual.windParticles.velocityOverLifetime;
        velocityModule.enabled = true;

        Vector3 windDir = -windVisual.transform.forward.normalized;
        float speed = 10f;

        velocityModule.x = new ParticleSystem.MinMaxCurve(windDir.x * speed);
        velocityModule.y = new ParticleSystem.MinMaxCurve(windDir.y * speed);
        velocityModule.z = new ParticleSystem.MinMaxCurve(windDir.z * speed);
    }

    private void PositionWindFX()
    {
        if (windVisual == null || playerObject == null || windZone == null)
            return;

        windVisual.PositionFX(playerObject.transform.position);
    }

    private void ScheduleNextGust()
    {
        nextGustTime = Time.time + Random.Range(minTimeBetweenGusts, maxTimeBetweenGusts);
    }

    public void TriggerGustManually()
    {
        if (!isGustActive)
        {
            StopAllCoroutines();
            StartCoroutine(TriggerWindGust());
        }
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

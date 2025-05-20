using UnityEngine;

public class WindVisualController : MonoBehaviour
{
    public ParticleSystem windParticles;
    public AudioSource windAudio;

    void Start()
    {
        if (windParticles != null)
        {
            var main = windParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
    }

    public void ShowWind()
    {
        if (windParticles != null && !windParticles.isPlaying)
            windParticles.Play();

        if (windAudio != null && !windAudio.isPlaying)
            windAudio.Play();
    }

    public void HideWind()
    {
        if (windParticles != null && windParticles.isPlaying)
            windParticles.Stop();

        if (windAudio != null && windAudio.isPlaying)
            windAudio.Stop();
    }

    public void SetParticleWindDirection(Vector3 windDir, float speed)
    {
        if (windParticles == null)
            return;

        var velocity = windParticles.velocityOverLifetime;
        velocity.enabled = true;

        windDir.Normalize();

        velocity.x = new ParticleSystem.MinMaxCurve(windDir.x * speed);
        velocity.y = new ParticleSystem.MinMaxCurve(windDir.y * speed);
        velocity.z = new ParticleSystem.MinMaxCurve(windDir.z * speed);
    }

    public void PositionFX(Vector3 playerPosition, Vector3 windDir, float forwardDistance, float sideOffset = 1f)
    {
        windDir.y = 0;
        windDir.Normalize();

        Vector3 spawnPos = playerPosition - windDir * forwardDistance;

        // Apply side offset to the right
        Vector3 rightOffset = Vector3.Cross(Vector3.up, windDir).normalized * sideOffset;
        spawnPos += rightOffset;

        transform.position = spawnPos;
        transform.rotation = Quaternion.LookRotation(windDir);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}

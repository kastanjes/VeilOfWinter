using System.Collections;
using UnityEngine;


public class WindVisualController : MonoBehaviour
{
    public ParticleSystem windParticles;
    public AudioSource windAudio;

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
        if (windParticles == null) return;

        var vel = windParticles.velocityOverLifetime;
        vel.enabled = true;
        windDir.Normalize();

        vel.x = new ParticleSystem.MinMaxCurve(windDir.x * speed);
        vel.y = new ParticleSystem.MinMaxCurve(windDir.y * speed);
        vel.z = new ParticleSystem.MinMaxCurve(windDir.z * speed);
    }

    public void PositionFX(Vector3 playerPos, Vector3 windDir, float forwardOffset, float sideOffset)
    {
        windDir.y = 0;
        windDir.Normalize();
        Vector3 spawnPos = playerPos + windDir * forwardOffset;
        Vector3 side = Vector3.Cross(Vector3.up, windDir).normalized * sideOffset;
        transform.position = spawnPos + side;
        transform.rotation = Quaternion.LookRotation(windDir);
    }

    public void TriggerWindAnimation(Animator animator)
    {
        if (animator == null) return;
        animator.ResetTrigger("WindTrigger");
        animator.SetTrigger("WindTrigger");
        Debug.Log("Triggered Wind Animation");
    }
}


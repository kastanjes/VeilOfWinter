// Assets/Scripts/WindVisualController.cs

using System.Collections;
using UnityEngine;

public class WindVisualController : MonoBehaviour
{
    public ParticleSystem windParticles;
    public AudioSource windAudio;

    [Header("Manual Offset Settings")]
    [Range(-10f, 10f)] public float verticalOffset = 1f;
    [Range(-10f, 10f)] public float horizontalOffset = 5f;
    [Range(-5f, 5f)] public float depthOffset = 0f;

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

    public void PositionFX(Vector3 playerPosition)
    {
        Vector3 spawnPos = playerPosition;
        spawnPos += new Vector3(horizontalOffset, verticalOffset, depthOffset);
        transform.position = spawnPos;
    }

    public void TriggerWindAnimation(Animator animator)
    {
        if (animator == null) return;
        animator.ResetTrigger("WindTrigger");
        animator.SetTrigger("WindTrigger");
        Debug.Log("Triggered Wind Animation");
    }
}

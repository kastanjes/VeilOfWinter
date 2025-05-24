using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider slider;

    void Start()
    {
        if (audioMixer != null && slider != null)
        {
            float volume;
            audioMixer.GetFloat("MasterVolume", out volume);
            slider.value = Mathf.Pow(10f, volume / 20f); // dB to linear
        }
    }

    public void SetVolume(float value)
    {
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", volume);
    }
}

using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;

    [Header("Global Volume Control")]
    public AudioMixer audioMixer;
    [Range(0.0001f, 1f)]
    public float masterVolume = 1f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];
        }
    }

    void Start()
    {
        SetMasterVolume(masterVolume);
        Play("Theme");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void PlayOneShot(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.PlayOneShot(s.source.clip, s.volume);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "EndScene")
        {
            StartCoroutine(FadeOutAndPlayNew("Theme", "EndTheme"));
        }
    }

    public IEnumerator FadeOutAndPlayNew(string oldTrack, string newTrack)
    {
        Sound oldSound = Array.Find(sounds, s => s.name == oldTrack);
        if (oldSound != null)
        {
            float startVolume = oldSound.source.volume;

            while (oldSound.source.volume > 0.01f)
            {
                oldSound.source.volume -= startVolume * Time.deltaTime / 2f;
                yield return null;
            }

            oldSound.source.Stop();
            oldSound.source.volume = startVolume;
        }

        yield return new WaitForSeconds(0.2f);
        Play(newTrack);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp(value, 0.0001f, 1f);
        float volume = Mathf.Log10(masterVolume) * 20f;
        audioMixer.SetFloat("MasterVolume", volume);
    }
}

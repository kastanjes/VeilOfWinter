using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    public static AudioManager instance;

    // Start is called before the first frame update
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
        }

    }

    void Start()
    {
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

    // Ny PlayOneShot metode
    public void PlayOneShot(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        // PlayOneShot afspiller lyden én gang uden at afbryde andre lyde
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

}
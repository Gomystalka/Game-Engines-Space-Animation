using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;

    public AudioClipGroup[] sfxClips;
    public AudioClipGroup[] musicClips;

    public static SoundSystem instance;

    private AudioClip _clickClip;

    private Dictionary<string, AudioClip> _clipCache;

    public static float SFXVolume {
        get => instance.sfxAudioSource.volume;
        set => instance.sfxAudioSource.volume = value;
    }
    public static float MusicVolume
    {
        get => instance.musicAudioSource.volume;
        set => instance.musicAudioSource.volume = value;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _clipCache = new Dictionary<string, AudioClip>();
        instance = Utilities.CreateSingleton(instance, this) as SoundSystem;
    }
    private void Start() => _clickClip = FindClipByName("Click");

    public static void PlaySound(AudioClip clip, byte playCount, bool waitForEnd, float delay = 0.2f) => instance.StartCoroutine(instance.PlaySoundRoutine(clip, playCount, waitForEnd, delay));
    private IEnumerator PlaySoundRoutine(AudioClip clip, byte playCount, bool waitForEnd, float delay = 0.2f)
    {
        byte index = 0;
        sfxAudioSource.clip = clip;
        while (index < playCount)
        {
            if (waitForEnd && sfxAudioSource.isPlaying) continue;
            if (waitForEnd)
                sfxAudioSource.Play();
            else
            {
                sfxAudioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(delay);
            }
            index++;
            yield return new WaitForEndOfFrame();
        }
    }

    public static void PlayMusic(AudioClip clip, bool loop) {
        instance.musicAudioSource.Stop();
        instance.musicAudioSource.clip = clip;
        instance.musicAudioSource.loop = loop;
        instance.musicAudioSource.Play();
    }

    public static AudioClip FindClipByName(string name) {
        name = name.ToLower();
        if (instance._clipCache.ContainsKey(name))
            return instance._clipCache[name];

        foreach (AudioClipGroup clip in instance.sfxClips) {
            if (clip.name.ToLower() == name)
            {
                instance._clipCache.Add(name, clip.clip);
                return clip.clip;
            }
        }
        foreach (AudioClipGroup clip in instance.musicClips)
        {
            if (clip.name.ToLower() == name)
            {
                instance._clipCache.Add(name, clip.clip);
                return clip.clip;
            }
        }
        return null;
    }

    public static void OnClickAnyUIElement() => PlaySound(instance._clickClip, 1, false, 0f);
    public void OnClickAnyUIElementNonStaticWrapper() => OnClickAnyUIElement();
}

[System.Serializable]
public class AudioClipGroup {
    public string name;
    public AudioClip clip;
}

using UnityEngine;

/// <summary>
/// Pure audio playback tool. No SignalManager dependency.
/// Place on a GameObject with an AudioSource component.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioLoader : MonoBehaviour
{
    AudioSource audioSource;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}

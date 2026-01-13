using UnityEngine;
using System.Collections;

public class MusicFader : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float duration, float targetVolume)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float startVolume = audioSource.volume;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    public static IEnumerator FadeOutAndStop(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}

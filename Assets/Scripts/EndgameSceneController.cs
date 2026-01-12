using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndgameSceneController : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;

    [Header("Transition Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 2f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(MusicFader.FadeOutAndStop(musicSource, 3f));
        StartCoroutine(EndGameMainMenuSequence());
    }

    void Start() {
        musicSource.Play();
        StartCoroutine(MusicFader.FadeIn(musicSource, 3f, 0.3f));
    }

    IEnumerator EndGameMainMenuSequence()
    {
        float t = 0;
        float startVolume = (musicSource != null) ? musicSource.volume : 0;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0;
        }

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / fadeDuration;

            if (musicSource != null)
                musicSource.volume = Mathf.Lerp(startVolume, 0, normalizedTime);

            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, normalizedTime);

            yield return null;
        }

        if (musicSource != null) musicSource.Stop();
        SceneManager.LoadScene("Load-in");
    }
}

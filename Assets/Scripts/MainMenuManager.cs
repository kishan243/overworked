using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI and Audio References")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] private float delayTime = 1f;

    private void Start()
    {
        StartCoroutine(MusicFader.FadeIn(audioSource, 3f, 0.3f));

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ClickPlay() => StartCoroutine(LoadSceneAfterDelay("Environment"));
    public void ClickInstructions() => StartCoroutine(LoadSceneAfterDelay("Instructions"));
    public void ClickManual() => StartCoroutine(LoadSceneAfterDelay("Manual"));

    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(MusicFader.FadeOutAndStop(audioSource, 1f));

            fadeCanvasGroup.blocksRaycasts = true;

            float elapsed = 0;
            while (elapsed < delayTime)
            {
                elapsed += Time.deltaTime;

                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / delayTime);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(delayTime);
        }

        SceneManager.LoadScene(sceneName);
    }
}

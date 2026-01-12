using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public LevelGenerator generator;
    public CanvasGroup fadeCanvasGroup;

    [Header("Audio References")]
    public AudioSource musicSource;
    public AudioSource warningSource;

    [Header("Settings")]
    public float timeRemaining = 60f;
    public float lowTimeThreshold = 30f;

    [Header("Transition Settings")]
    public float fadeDuration = 2f;

    private bool timerIsRunning = true;
    private bool isFlashing = false;
    private bool isEnding = false;

    void Update()
    {
        if (generator != null && generator.isDoneGenerating)
        {
            if (timerIsRunning)
            {
                if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime;
                    DisplayTime(timeRemaining);

                    if (timeRemaining <= lowTimeThreshold && !isFlashing)
                    {
                        StartCoroutine(FlashLowTime());
                    }
                }
                else if (!isEnding)
                {
                    OnTimerEnd();
                }
            }
        }
    }

    private void OnTimerEnd()
    {
        isEnding = true;
        timeRemaining = 0;
        timerIsRunning = false;
        DisplayTime(0);
        StopAllCoroutines();

        if (warningSource != null) warningSource.Stop();

        timerText.color = Color.red;
        StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
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
        SceneManager.LoadScene("Endgame");
    }

    public void AddTime(float secondsToAdd)
    {
        if (isEnding) return;
        timeRemaining += secondsToAdd;

        if (timeRemaining > lowTimeThreshold && isFlashing)
        {
            StopAllCoroutines();
            isFlashing = false;
            timerText.color = Color.white;

            if (warningSource != null) warningSource.Stop();
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    IEnumerator FlashLowTime()
    {
        isFlashing = true;

        if (warningSource != null)
        {
            warningSource.Play();
        }

        while (isFlashing)
        {
            timerText.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            timerText.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
    }
}

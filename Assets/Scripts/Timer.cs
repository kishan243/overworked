using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Timer : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    [Header("Settings")]
    public float timeRemaining = 60f;
    public float lowTimeThreshold = 30f;

    private bool timerIsRunning = true;
    private bool isFlashing = false;

    void Update()
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
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                DisplayTime(0);
                StopAllCoroutines();
                timerText.color = Color.red;
            }
        }
    }

    public void AddTime(float secondsToAdd)
    {
        timeRemaining += secondsToAdd;

        if (timeRemaining > lowTimeThreshold && isFlashing)
        {
            StopAllCoroutines();
            isFlashing = false;
            timerText.color = Color.white;
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    System.Collections.IEnumerator FlashLowTime()
    {
        isFlashing = true;
        while (isFlashing)
        {
            timerText.color = Color.red;
            yield return new WaitForSeconds(0.5f);

            timerText.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
    }
}

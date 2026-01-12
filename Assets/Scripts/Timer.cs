using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [Header("UI and Level Generator Reference")]
    public TextMeshProUGUI timerText;
    public LevelGenerator generator;

    [Header("Settings")]
    public float timeRemaining = 60f;
    public float lowTimeThreshold = 30f;
    public string endGameSceneName = "EndGame"; // Name of your end game scene

    private bool timerIsRunning = true;
    private bool isFlashing = false;
    private bool gameEnded = false;

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
                else
                {
                    timeRemaining = 0;
                    timerIsRunning = false;
                    DisplayTime(0);
                    StopAllCoroutines();
                    timerText.color = Color.red;

                    // End the game
                    if (!gameEnded)
                    {
                        gameEnded = true;
                        EndGame();
                    }
                }
            }
        }
    }

    void EndGame()
    {
        Debug.Log("⏰ Time's up! Going to end game scene...");

        // Wait a brief moment so player can see the timer hit zero
        Invoke("LoadEndGameScene", 1.5f);
    }

    void LoadEndGameScene()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.GoToEndGame();
        }
        else
        {
            // Fallback if GameDataManager doesn't exist
            SceneManager.LoadScene(endGameSceneName);
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
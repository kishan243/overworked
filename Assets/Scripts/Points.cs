using UnityEngine;
using TMPro;
using System.Collections;

public class Points : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI feedbackText;

    [Header("Settings")]
    public int totalPoints = 0;
    public int giftsCompleted = 0;
    public float displayDuration = 1.0f;

    void Start()
    {
        UpdateDisplay();
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    public void AddPoints(int points)
    {
        totalPoints += points;
        giftsCompleted++;
        UpdateDisplay();

        PlayerPrefs.SetInt("GiftsCompleted", giftsCompleted);
        PlayerPrefs.SetInt("FinalPoints", totalPoints);

        if (feedbackText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowFeedback($"+{points}"));
        }
    }

    private IEnumerator ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.color = Color.green;
        feedbackText.gameObject.SetActive(true);

        float elapsed = 0;
        Vector3 originalPos = feedbackText.transform.localPosition;
        Vector3 targetPos = originalPos + new Vector3(0, 20f, 0);

        while (elapsed < displayDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / displayDuration);
            feedbackText.color = new Color(0, 1, 0, alpha);

            feedbackText.transform.localPosition = Vector3.Lerp(originalPos, targetPos, elapsed / displayDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
        feedbackText.transform.localPosition = originalPos;
    }

    private void UpdateDisplay()
    {
        pointsText.text = $"{totalPoints} points";
    }
}

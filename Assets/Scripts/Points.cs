using UnityEngine;
using TMPro;

public class Points : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI pointsText;

    [Header("Settings")]
    public int totalPoints = 0;

    void Start()
    {
        UpdateDisplay();
    }

    public void AddPoints(int points)
    {
        totalPoints += points;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        pointsText.text = $"{totalPoints} points";
    }

    public int GetTotalPoints()
    {
        return totalPoints;
    }
}
using UnityEngine;
using TMPro;

public class EndGameDisplay : MonoBehaviour
{
    [Header("Drag Your Existing UI Text Here")]
    public TextMeshProUGUI giftsTotal;
    public TextMeshProUGUI pointsTotal;

    void Start()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (GameDataManager.Instance != null)
        {
            // Update the existing UI text with the totals
            if (giftsTotal != null)
                giftsTotal.text = GameDataManager.Instance.totalGiftsDelivered.ToString()+ "Gifts";

            if (pointsTotal != null)
                pointsTotal.text = GameDataManager.Instance.totalPoints.ToString() + "Points";

            Debug.Log($"📊 Displaying - Gifts: {GameDataManager.Instance.totalGiftsDelivered}, Points: {GameDataManager.Instance.totalPoints}");
        }
        else
        {
            Debug.LogWarning("⚠️ GameDataManager not found!");
            if (giftsTotal != null) giftsTotal.text = "0";
            if (pointsTotal != null) pointsTotal.text = "0";
        }
    }
}
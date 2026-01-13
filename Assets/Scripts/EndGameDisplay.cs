using UnityEngine;
using TMPro;

public class EndGameDisplay : MonoBehaviour
{
    [Header("UI References")]
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
            if (giftsTotal != null)
                giftsTotal.text = GameDataManager.Instance.totalGiftsDelivered.ToString();

            if (pointsTotal != null)
                pointsTotal.text = GameDataManager.Instance.totalPoints.ToString() + " Points";
        }
        else
        {
            if (giftsTotal != null) giftsTotal.text = "0";
            if (pointsTotal != null) pointsTotal.text = "0";
        }
    }
}

using UnityEngine;
using TMPro;

public class Quota : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI quotaText;

    [Header("Settings")]
    public int quotaGoal = 10;
    public int sessionQuota = 0;

    void Start()
    {
        UpdateDisplay();
    }

    public void QuotaProgressOne(int amount = 1)
    {
        sessionQuota += amount;

        if (GameDataManager.Instance != null)
        {
            for (int i = 0; i < amount; i++)
            {
                GameDataManager.Instance.AddGift();
            }
        }

        if (sessionQuota > quotaGoal) sessionQuota = quotaGoal;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int displayQuota = sessionQuota;
        if (GameDataManager.Instance != null)
        {
            displayQuota = GameDataManager.Instance.totalGiftsDelivered;
            if (displayQuota > quotaGoal) displayQuota = quotaGoal;
        }

        quotaText.text = $"{displayQuota}/{quotaGoal}";
    }
}
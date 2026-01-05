using UnityEngine;
using TMPro;

public class Quota : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI quotaText;

    [Header("Settings")]
    public int quotaGoal = 10;
    private int quotaFulfilled = 0;

    void Start()
    {
        UpdateDisplay();
    }

    public void QuotaProgressOne(int amount = 1)
    {
        quotaFulfilled += amount;
        if (quotaFulfilled > quotaGoal) quotaFulfilled = quotaGoal;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        quotaText.text = $"{quotaFulfilled}/{quotaGoal}";
    }
}

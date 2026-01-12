using UnityEngine;
using TMPro;

public class EndgamePointsDisplay : MonoBehaviour
{
    public TextMeshProUGUI resultText;

    void Start()
    {
        int gifts = PlayerPrefs.GetInt("GiftsCompleted", 0);

        if (resultText != null)
        {
            resultText.text = $"({gifts} points)";
        }
    }
}

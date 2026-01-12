using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("Game Stats")]
    public int totalPoints = 0;
    public int totalGiftsDelivered = 0;

    void Awake()
    {
        // Singleton pattern - only one instance exists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GameDataManager created and persisting across scenes");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddPoints(int points)
    {
        totalPoints += points;
        Debug.Log($"📊 Total Points: {totalPoints}");
    }

    public void AddGift()
    {
        totalGiftsDelivered++;
        Debug.Log($"🎁 Total Gifts: {totalGiftsDelivered}");
    }

    public void ResetStats()
    {
        totalPoints = 0;
        totalGiftsDelivered = 0;
        Debug.Log("🔄 Game stats reset");
    }

    public void GoToEndGame()
    {
        SceneManager.LoadScene("EndGame"); // Make sure you have an "EndGame" scene
    }
}
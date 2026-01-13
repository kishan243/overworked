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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
    }

    public void AddGift()
    {
        totalGiftsDelivered++;
    }

    public void ResetStats()
    {
        totalPoints = 0;
        totalGiftsDelivered = 0;
    }

    public void GoToEndGame()
    {
        SceneManager.LoadScene("EndGame"); 
    }
}
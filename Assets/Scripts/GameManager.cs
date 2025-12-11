using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float gameTime = 0f;
    public bool gameStarted = false;
    public bool gameEnded = false;

    private ToyManager toyManager;
    private UIManager uiManager;

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        toyManager = ToyManager.Instance;
        uiManager = UIManager.Instance;
        
        StartGame();
    }

    void Update()
    {
        if (gameStarted && !gameEnded)
        {
            gameTime += Time.deltaTime;
            
            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateTimer(gameTime);
                
                if (toyManager != null)
                {
                    uiManager.UpdateToyList(toyManager.toyList);
                    uiManager.UpdateCompletedCount(
                        toyManager.GetCompletedToyCount(), 
                        toyManager.totalToysToMake
                    );
                }
            }

            // Check win condition
            if (toyManager != null && toyManager.AreAllToysCompleted())
            {
                EndGame();
            }
        }
    }

    void StartGame()
    {
        gameStarted = true;
        gameTime = 0f;
        Debug.Log("Game Started! Make all the toys!");
    }

    void EndGame()
    {
        gameEnded = true;
        Debug.Log("Game Over! Final Time: " + gameTime);
        
        if (uiManager != null)
        {
            uiManager.ShowGameOver(gameTime);
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

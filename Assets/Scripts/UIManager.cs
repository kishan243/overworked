using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public Text timerText;
    public Text toyListText;
    public Text completedCountText;
    public Text gameOverText;
    public Text instructionsText;

    private static UIManager instance;
    public static UIManager Instance { get { return instance; } }

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
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        
        UpdateInstructions();
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
            timerText.text = string.Format("Time: {0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
    }

    public void UpdateToyList(List<Toy> toys)
    {
        if (toyListText != null)
        {
            string listText = "Toys to Make:\n";
            for (int i = 0; i < toys.Count; i++)
            {
                Toy toy = toys[i];
                string status = toy.isCompleted ? "[COMPLETE]" : 
                    string.Format("[{0}/{1}]", toy.currentComponentIndex, toy.requiredComponents.Count);
                listText += string.Format("{0}. {1} {2}\n", i + 1, toy.toyName, status);
                
                if (!toy.isCompleted && toy.currentComponentIndex < toy.requiredComponents.Count)
                {
                    listText += string.Format("   Next: {0}\n", toy.GetNextRequiredComponent());
                }
            }
            toyListText.text = listText;
        }
    }

    public void UpdateCompletedCount(int completed, int total)
    {
        if (completedCountText != null)
        {
            completedCountText.text = string.Format("Completed: {0}/{1}", completed, total);
        }
    }

    public void ShowGameOver(float finalTime)
    {
        if (gameOverText != null)
        {
            int minutes = Mathf.FloorToInt(finalTime / 60f);
            int seconds = Mathf.FloorToInt(finalTime % 60f);
            gameOverText.text = string.Format("All Toys Completed!\nFinal Time: {0:00}:{1:00}", minutes, seconds);
            gameOverText.gameObject.SetActive(true);
        }
    }

    void UpdateInstructions()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "WASD - Move | E - Interact | SPACE - Swap Characters";
        }
    }
}

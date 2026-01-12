using UnityEngine;
using TMPro;

public class AIModeDisplay : MonoBehaviour
{
    public TextMeshProUGUI modeText;

    private AICompanion ai;

    void Start()
    {
        // Find AI every frame if needed
        Debug.Log("🖥️ AIModeDisplay started");
    }

    void Update()
    {
        // Always try to find AI if we don't have it
        if (ai == null)
        {
            ai = FindObjectOfType<AICompanion>();
            if (ai == null) return;
        }

        if (modeText == null)
        {
            Debug.LogError("❌ Mode Text is not assigned!");
            return;
        }

        // Update display
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        string text = "AI Mode: ";

        switch (ai.currentMode)
        {
            case AICompanion.AgentMode.PLAOnly:
                text += "PLA TOYS ONLY";
                modeText.color = Color.cyan;
                break;

            case AICompanion.AgentMode.LeatherOnly:
                text += "LEATHER TOYS ONLY";
                modeText.color = Color.yellow;
                break;

            case AICompanion.AgentMode.Manual:
                text += "MANUAL COMMAND";
                modeText.color = Color.green;

                if (ai.selectedRecipeIndex >= 0)
                {
                    text += $"\n[Recipe {ai.selectedRecipeIndex + 1} Selected - Press K to Assign]";
                }
                else
                {
                    text += "\n[Press 1-4 to select recipe]";
                }
                break;
        }

        modeText.text = text;
    }
}
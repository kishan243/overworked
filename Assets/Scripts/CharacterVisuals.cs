using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual feedback for characters showing what they're holding.
/// </summary>
public class CharacterVisuals : MonoBehaviour
{
    public PlayerController playerController;
    public AIController aiController;
    public GameObject heldItemIndicator;
    public Text heldItemText;
    public GameObject activeIndicator;
    public Color playerColor = new Color(0.2f, 0.4f, 1.0f);
    public Color aiColor = new Color(1.0f, 0.3f, 0.2f);

    private Renderer characterRenderer;
    private MaterialPropertyBlock propertyBlock;
    private bool isPlayer;

    void Awake()
    {
        characterRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        isPlayer = playerController != null;
    }

    void Start()
    {
        SetCharacterColor();
        
        if (heldItemIndicator != null)
            heldItemIndicator.SetActive(false);
    }

    void Update()
    {
        UpdateHeldItem();
        UpdateActiveIndicator();
    }

    void SetCharacterColor()
    {
        if (characterRenderer == null)
            return;

        Color targetColor = isPlayer ? playerColor : aiColor;
        
        characterRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", targetColor);
        characterRenderer.SetPropertyBlock(propertyBlock);
    }

    void UpdateHeldItem()
    {
        string heldComponent = "";
        
        if (playerController != null)
            heldComponent = playerController.GetHeldComponent();
        else if (aiController != null)
            heldComponent = aiController.GetHeldComponent();

        bool hasItem = !string.IsNullOrEmpty(heldComponent);
        
        if (heldItemIndicator != null)
            heldItemIndicator.SetActive(hasItem);
        
        if (heldItemText != null && hasItem)
            heldItemText.text = heldComponent;
    }

    void UpdateActiveIndicator()
    {
        if (activeIndicator == null)
            return;

        bool isActive = false;
        
        if (playerController != null)
            isActive = playerController.isPlayerControlled;
        else if (aiController != null)
            isActive = !aiController.isAIControlled;

        activeIndicator.SetActive(isActive);
    }
}

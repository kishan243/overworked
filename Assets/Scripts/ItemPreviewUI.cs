using UnityEngine;
using UnityEngine.UI;

public class ItemPreviewUI : MonoBehaviour
{
    [Header("UI References")]
    public RawImage previewImage;
    public RectTransform borderRect;

    [Header("Border Colors")]
    public Color heldColor = Color.green;
    public Color nearbyColor = Color.yellow;

    [Header("Camera Settings")]
    public Vector3 cameraOffset = new Vector3(0, 0, -3f);
    public float cameraSize = 1.5f;

    [Header("Item Sprites - Assign All Possible Items Here")]
    public Sprite blueToySprite;
    public Sprite yellowToySprite;
    public Sprite greenToySprite;
    public Sprite bluePLASprite;
    public Sprite yellowPLASprite;
    public Sprite greenPLASprite;
    public Sprite brownLeatherSprite;
    public Sprite purpleLeatherSprite;
    public Sprite silverLeatherSprite;
    public Sprite yellowLeatherSprite;
    public Sprite footballSprite;
    public Sprite hatSprite;
    public Sprite backpackSprite;
    public Sprite defaultSprite;

    [Header("Use Sprites Instead of 3D?")]
    public bool useSpriteMode = false;
    public Image spriteImage; // Assign if using sprite mode

    private Camera renderCamera;
    private RenderTexture renderTexture;
    private GameObject currentPreviewObject;
    private Image borderImage;

    void Start()
    {
        if (!useSpriteMode)
        {
            renderTexture = new RenderTexture(256, 256, 16);
            renderTexture.Create();
            previewImage.texture = renderTexture;

            GameObject camObj = new GameObject("ItemPreviewCamera");
            camObj.transform.SetParent(transform);
            renderCamera = camObj.AddComponent<Camera>();
            renderCamera.targetTexture = renderTexture;
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = cameraSize;
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            renderCamera.cullingMask = 1 << LayerMask.NameToLayer("ItemPreview");
            renderCamera.enabled = true;
        }

        if (borderRect != null)
        {
            borderImage = borderRect.GetComponent<Image>();
        }

        HidePreview();

        Debug.Log($"ItemPreviewUI Setup - Sprite Mode: {useSpriteMode}, Sprite Image: {spriteImage != null}, Border: {borderImage != null}");
    }

    public void ShowPreview(GameObject prefab, bool isHeld)
    {
        if (prefab == null)
        {
            HidePreview();
            return;
        }

        if (useSpriteMode && spriteImage != null)
        {
            // Sprite mode
            Sprite sprite = GetSpriteForItem(prefab.name);

            Debug.Log($"ShowPreview: {prefab.name}, Found sprite: {sprite != null}, IsHeld: {isHeld}");

            if (sprite != null)
            {
                spriteImage.sprite = sprite;
                spriteImage.enabled = true;
                spriteImage.gameObject.SetActive(true);

                if (borderImage != null)
                {
                    borderImage.color = isHeld ? heldColor : nearbyColor;
                    borderImage.enabled = true;
                    borderImage.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning($"No sprite found for: {prefab.name}");
                HidePreview();
            }
        }
        else
        {
            // 3D mode (original)
            if (currentPreviewObject != null)
            {
                Destroy(currentPreviewObject);
            }

            currentPreviewObject = Instantiate(prefab);
            currentPreviewObject.transform.position = renderCamera.transform.position + cameraOffset;
            currentPreviewObject.transform.rotation = Quaternion.Euler(15, 45, 0);

            SetLayerRecursively(currentPreviewObject, LayerMask.NameToLayer("ItemPreview"));

            Rigidbody rb = currentPreviewObject.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            Collider col = currentPreviewObject.GetComponent<Collider>();
            if (col != null) Destroy(col);

            RotatePreview rotator = currentPreviewObject.AddComponent<RotatePreview>();
            rotator.rotationSpeed = 30f;

            if (borderImage != null)
            {
                borderImage.color = isHeld ? heldColor : nearbyColor;
            }

            previewImage.gameObject.SetActive(true);
            if (borderRect != null) borderRect.gameObject.SetActive(true);
        }
    }

    public void HidePreview()
    {
        if (currentPreviewObject != null)
        {
            Destroy(currentPreviewObject);
            currentPreviewObject = null;
        }

        if (previewImage != null)
        {
            previewImage.enabled = false;
            previewImage.gameObject.SetActive(false);
        }
        if (spriteImage != null)
        {
            spriteImage.enabled = false;
            spriteImage.gameObject.SetActive(false);
        }
        if (borderImage != null)
        {
            borderImage.enabled = false;
            borderImage.gameObject.SetActive(false);
        }
        if (borderRect != null)
        {
            borderRect.gameObject.SetActive(false);
        }
    }

    Sprite GetSpriteForItem(string itemName)
    {
        string name = itemName.ToLower().Replace("(clone)", "").Trim();

        // Toys
        if (name.Contains("ToyBoat")) return blueToySprite;
        if (name.Contains("ToySteamroller")) return yellowToySprite;
        if (name.Contains("ToyBricks")) return greenToySprite;

        // PLA
        if (name.Contains("pla") && name.Contains("blue")) return bluePLASprite;
        if (name.Contains("pla") && name.Contains("yellow")) return yellowPLASprite;
        if (name.Contains("pla") && name.Contains("green")) return greenPLASprite;

        // Leather
        if (name.Contains("brown") && name.Contains("leather")) return brownLeatherSprite;
        if (name.Contains("purple") && name.Contains("leather")) return purpleLeatherSprite;
        if (name.Contains("silver") && name.Contains("leather")) return silverLeatherSprite;
        if (name.Contains("gold") && name.Contains("leather")) return yellowLeatherSprite;

        // Finished items
        if (name.Contains("football")) return footballSprite;
        if (name.Contains("hat")) return hatSprite;
        if (name.Contains("backpack")) return backpackSprite;

        return defaultSprite;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void OnDestroy()
    {
        if (currentPreviewObject != null)
        {
            Destroy(currentPreviewObject);
        }
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}
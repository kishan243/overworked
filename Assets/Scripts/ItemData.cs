using UnityEngine;

public class ItemData : MonoBehaviour
{
    public enum ItemType
    {
        RedPLA,
        BluePLA,
        GreenPLA,
        YellowPLA,
        BlackPLA,
        WhitePLA
    }

    [Header("Item Settings")]
    public ItemType itemType;
    public GameObject itemPrefab;

    [Header("Visual Feedback (Optional)")]
    public bool enableGlow = false;
    public Color glowColor = Color.white;

    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
        }
    }

    void Update()
    {
        if (!enableGlow || rend == null || mat == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < 2.5f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glowColor * 0.5f);
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
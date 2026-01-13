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
}
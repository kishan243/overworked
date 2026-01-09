using UnityEngine;

public class LeatherData : MonoBehaviour
{
    public enum LeatherType
    {
        Brown,
        Purple,
        Silver,
        Yellow
    }

    [Header("Leather Settings")]
    public LeatherType leatherType;
    public GameObject leatherPrefab;  // Optional - leave empty to use this object itself
}

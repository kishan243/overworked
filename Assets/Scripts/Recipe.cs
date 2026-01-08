using UnityEngine;

// Attach this to each ORDER TICKET (recipe UI prefab)
// This tells the player what toy they need to make and submit
public class Recipe : MonoBehaviour
{
    [Header("What Toy Is Needed?")]
    public string toyName = ""; // e.g. "BlueToy", "YellowToy", "GreenToy", "Football", "Hat", "Backpack"

    [Header("Points Settings")]
    public int maxPoints = 100; // Points if completed instantly
    public int minPoints = 20;  // Points if completed at max time
    public float maxTime = 60f; // Time until points reach minimum

    [HideInInspector]
    public float spawnTime; // When this recipe was created

    void Start()
    {
        spawnTime = Time.time;
    }

    public int GetPointsForCompletion()
    {
        float elapsed = Time.time - spawnTime;
        float t = Mathf.Clamp01(elapsed / maxTime);

        // Lerp from maxPoints to minPoints based on time
        int points = Mathf.RoundToInt(Mathf.Lerp(maxPoints, minPoints, t));
        return points;
    }
}
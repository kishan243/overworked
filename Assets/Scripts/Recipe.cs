using UnityEngine;

public class Recipe : MonoBehaviour
{
    [Header("What Toy Is Needed?")]
    public string toyName = ""; 

    [Header("Points Settings")]
    public int maxPoints = 100; 
    public int minPoints = 20; 
    public float maxTime = 60f; 

    [HideInInspector]
    public float spawnTime; 

    void Start()
    {
        spawnTime = Time.time;
    }

    public int GetPointsForCompletion()
    {
        float elapsed = Time.time - spawnTime;
        float t = Mathf.Clamp01(elapsed / maxTime);

        int points = Mathf.RoundToInt(Mathf.Lerp(maxPoints, minPoints, t));
        return points;
    }
}
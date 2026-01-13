using UnityEngine;
using System.Collections.Generic;

public class RecipeManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Transform ticketTray;
    public List<GameObject> recipePrefabs;

    [Header("Settings")]
    public int maxTickets = 4;

    void Start()
    {
        foreach (Transform child in ticketTray) Destroy(child.gameObject);
        for (int i = 0; i < maxTickets; i++) SpawnRandomRecipe();
    }

    void SpawnRandomRecipe()
    {
        if (recipePrefabs == null || recipePrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, recipePrefabs.Count);
        GameObject newTicket = Instantiate(recipePrefabs[randomIndex], ticketTray);
        newTicket.transform.localScale = Vector3.one;

        Recipe recipe = newTicket.GetComponent<Recipe>();
        if (recipe != null) recipe.spawnTime = Time.time;
    }

    public bool HasMatchingRecipe(string toyName)
    {
        foreach (Transform child in ticketTray)
        {
            Recipe recipe = child.GetComponent<Recipe>();
            if (recipe != null && recipe.toyName.Equals(toyName, System.StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    public int CompleteRecipe(string toyName)
    {
        foreach (Transform child in ticketTray)
        {
            Recipe recipe = child.GetComponent<Recipe>();
            if (recipe != null && recipe.toyName.Equals(toyName, System.StringComparison.OrdinalIgnoreCase))
            {
                int points = recipe.GetPointsForCompletion();
                DestroyImmediate(child.gameObject);
                SpawnRandomRecipe();
                return points;
            }
        }
        return 0;
    }
}
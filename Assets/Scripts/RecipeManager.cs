using UnityEngine;
using System.Collections.Generic;

public class RecipeManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Transform ticketTray;
    public List<GameObject> recipePrefabs;

    [Header("Settings")]
    public int maxTickets = 3;

    void Start()
    {
        // Spawn initial 3 recipes
        for (int i = 0; i < maxTickets; i++)
        {
            SpawnRandomRecipe();
        }
    }

    void SpawnRandomRecipe()
    {
        if (ticketTray.childCount >= maxTickets) return;

        int randomIndex = Random.Range(0, recipePrefabs.Count);
        GameObject newTicket = Instantiate(recipePrefabs[randomIndex], ticketTray);
        newTicket.transform.localScale = Vector3.one;

        // Initialize spawn time
        Recipe recipe = newTicket.GetComponent<Recipe>();
        if (recipe != null)
        {
            recipe.spawnTime = Time.time;
        }
    }

    // Check if ANY active recipe matches this toy name
    public bool HasMatchingRecipe(string toyName)
    {
        foreach (Transform child in ticketTray)
        {
            Recipe recipe = child.GetComponent<Recipe>();
            if (recipe != null && recipe.toyName.Equals(toyName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    // Complete a recipe and return the points earned
    public int CompleteRecipe(string toyName)
    {
        foreach (Transform child in ticketTray)
        {
            Recipe recipe = child.GetComponent<Recipe>();
            if (recipe != null && recipe.toyName.Equals(toyName, System.StringComparison.OrdinalIgnoreCase))
            {
                int points = recipe.GetPointsForCompletion();
                Destroy(child.gameObject);
                SpawnRandomRecipe();
                return points; // Return points for this recipe
            }
        }
        return 0; // No matching recipe found
    }
}
using UnityEngine;
using System.Collections.Generic;

public class ToyManager : MonoBehaviour
{
    public List<Toy> toyList;
    public int totalToysToMake = 5;

    private static ToyManager instance;
    public static ToyManager Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        GenerateToyList();
    }

    void GenerateToyList()
    {
        toyList = new List<Toy>();
        
        // Define possible toy types with their required components
        string[][] toyTypes = new string[][]
        {
            new string[] { "Toy Car", "Wood", "Wheels", "Paint" },
            new string[] { "Toy Train", "Wood", "Wheels", "Wheels", "Paint" },
            new string[] { "Teddy Bear", "Fabric", "Stuffing", "Eyes", "Bow" },
            new string[] { "Robot", "Metal", "Metal", "Circuits", "Paint" },
            new string[] { "Doll", "Fabric", "Stuffing", "Hair", "Dress" }
        };

        // Randomly select toys from the pool
        for (int i = 0; i < totalToysToMake; i++)
        {
            int randomIndex = Random.Range(0, toyTypes.Length);
            string[] selectedToy = toyTypes[randomIndex];
            
            List<string> components = new List<string>();
            for (int j = 1; j < selectedToy.Length; j++)
            {
                components.Add(selectedToy[j]);
            }
            
            toyList.Add(new Toy(selectedToy[0], components));
        }
    }

    public bool AddComponentToCurrentToy(string componentType)
    {
        foreach (Toy toy in toyList)
        {
            if (!toy.isCompleted)
            {
                return toy.AddComponent(componentType);
            }
        }
        return false;
    }

    public Toy GetCurrentToy()
    {
        foreach (Toy toy in toyList)
        {
            if (!toy.isCompleted)
            {
                return toy;
            }
        }
        return null;
    }

    public bool AreAllToysCompleted()
    {
        foreach (Toy toy in toyList)
        {
            if (!toy.isCompleted)
                return false;
        }
        return true;
    }

    public int GetCompletedToyCount()
    {
        int count = 0;
        foreach (Toy toy in toyList)
        {
            if (toy.isCompleted)
                count++;
        }
        return count;
    }
}

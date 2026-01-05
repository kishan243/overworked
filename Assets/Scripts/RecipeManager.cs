using UnityEngine;
using System.Collections.Generic;

public class RecipeManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Transform ticketTray;
    public List<GameObject> recipePrefabs;

    [Header("Settings")]
    public int maxTickets = 5;

    void Start()
    {
        for (int i = 0; i < maxTickets; i++)
        {
            SpawnRandomRecipe();
        }
    }

    public void SpawnRandomRecipe()
    {

        if (ticketTray.childCount >= maxTickets) return;

        int randomIndex = Random.Range(0, recipePrefabs.Count);
        GameObject prefabToSpawn = recipePrefabs[randomIndex];

        GameObject newTicket = Instantiate(prefabToSpawn, ticketTray);

        newTicket.transform.localScale = Vector3.one;
    }

    public void RemoveTicket(int index)
    {
        if (index >= 0 && index < ticketTray.childCount)
        {
            GameObject ticket = ticketTray.GetChild(index).gameObject;

            Destroy(ticket);

            ticket.transform.SetParent(null);

            SpawnRandomRecipe();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RemoveTicket(0);
        }
    }
}

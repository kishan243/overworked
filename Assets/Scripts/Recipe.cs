using UnityEngine;

// Attach this to each ORDER TICKET (recipe UI prefab)
// This tells the player what toy they need to make and submit
public class Recipe : MonoBehaviour
{
    [Header("What Toy Is Needed?")]
    public string toyName = ""; // e.g. "BlueToy", "YellowToy", "GreenToy", "Football", "Hat", "Backpack"
}
using UnityEngine;

public class WorkStation : MonoBehaviour
{
    public string stationType; // e.g., "Wood", "Paint", "Wheels", "Assembly"
    public string componentProduced;
    public float interactionTime = 1f;

    private bool isOccupied = false;
    private float interactionTimer = 0f;
    private Transform currentUser;

    public bool IsAvailable()
    {
        return !isOccupied;
    }

    public bool StartInteraction(Transform user)
    {
        if (isOccupied)
            return false;

        isOccupied = true;
        currentUser = user;
        interactionTimer = 0f;
        return true;
    }

    public bool UpdateInteraction(float deltaTime)
    {
        if (!isOccupied)
            return false;

        interactionTimer += deltaTime;
        return interactionTimer >= interactionTime;
    }

    public string CompleteInteraction()
    {
        if (!isOccupied)
            return "";

        isOccupied = false;
        currentUser = null;
        interactionTimer = 0f;
        return componentProduced;
    }

    public void CancelInteraction()
    {
        isOccupied = false;
        currentUser = null;
        interactionTimer = 0f;
    }

    public float GetProgress()
    {
        return interactionTimer / interactionTime;
    }
}

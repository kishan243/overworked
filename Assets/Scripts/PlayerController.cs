using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool isPlayerControlled = true;
    public string characterName = "Player";

    private WorkStation currentStation;
    private bool isInteracting = false;
    private string heldComponent = "";

    void Update()
    {
        if (isPlayerControlled)
        {
            HandleMovement();
            HandleInteraction();
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
        
        if (movement.magnitude > 0.1f && !isInteracting)
        {
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isInteracting && currentStation != null)
            {
                // Complete interaction
                if (currentStation.UpdateInteraction(0f))
                {
                    heldComponent = currentStation.CompleteInteraction();
                    isInteracting = false;
                    Debug.Log(characterName + " picked up: " + heldComponent);
                }
            }
            else
            {
                // Try to start interaction with nearby station
                TryInteractWithStation();
            }
        }

        // Update ongoing interaction
        if (isInteracting && currentStation != null)
        {
            if (currentStation.UpdateInteraction(Time.deltaTime))
            {
                heldComponent = currentStation.CompleteInteraction();
                isInteracting = false;
                Debug.Log(characterName + " picked up: " + heldComponent);
            }
        }
    }

    void TryInteractWithStation()
    {
        WorkStation[] stations = FindObjectsOfType<WorkStation>();
        float minDistance = 2f;
        WorkStation nearestStation = null;

        foreach (WorkStation station in stations)
        {
            float distance = Vector3.Distance(transform.position, station.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestStation = station;
            }
        }

        if (nearestStation != null && nearestStation.IsAvailable())
        {
            // If holding a component and station is assembly, submit it
            if (nearestStation.stationType == "Assembly" && !string.IsNullOrEmpty(heldComponent))
            {
                if (ToyManager.Instance != null)
                {
                    if (ToyManager.Instance.AddComponentToCurrentToy(heldComponent))
                    {
                        Debug.Log(characterName + " added " + heldComponent + " to toy!");
                        heldComponent = "";
                    }
                    else
                    {
                        Debug.Log("Wrong component for current toy!");
                    }
                }
            }
            else if (string.IsNullOrEmpty(heldComponent))
            {
                // Start crafting a component
                if (nearestStation.StartInteraction(transform))
                {
                    currentStation = nearestStation;
                    isInteracting = true;
                    Debug.Log(characterName + " started interacting with " + nearestStation.stationType);
                }
            }
        }
    }

    public void SetPlayerControlled(bool controlled)
    {
        isPlayerControlled = controlled;
    }

    public string GetHeldComponent()
    {
        return heldComponent;
    }

    public bool IsInteracting()
    {
        return isInteracting;
    }
}

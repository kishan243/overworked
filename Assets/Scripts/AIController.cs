using UnityEngine;

public class AIController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool isAIControlled = true;
    public string characterName = "AI";

    private WorkStation currentStation;
    private bool isInteracting = false;
    private string heldComponent = "";
    private float decisionTimer = 0f;
    private float decisionInterval = 2f;

    void Update()
    {
        if (isAIControlled)
        {
            HandleAIBehavior();
        }
    }

    void HandleAIBehavior()
    {
        decisionTimer += Time.deltaTime;

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
        else if (decisionTimer >= decisionInterval)
        {
            decisionTimer = 0f;
            MakeDecision();
        }
    }

    void MakeDecision()
    {
        if (ToyManager.Instance == null)
            return;

        Toy currentToy = ToyManager.Instance.GetCurrentToy();
        if (currentToy == null)
            return;

        // If holding a component, deliver it
        if (!string.IsNullOrEmpty(heldComponent))
        {
            DeliverComponent();
        }
        else
        {
            // Get next required component and craft it
            string nextComponent = currentToy.GetNextRequiredComponent();
            if (!string.IsNullOrEmpty(nextComponent))
            {
                CraftComponent(nextComponent);
            }
        }
    }

    void DeliverComponent()
    {
        WorkStation[] stations = FindObjectsOfType<WorkStation>();
        WorkStation assemblyStation = null;

        foreach (WorkStation station in stations)
        {
            if (station.stationType == "Assembly")
            {
                assemblyStation = station;
                break;
            }
        }

        if (assemblyStation != null)
        {
            // Move towards assembly station
            Vector3 direction = (assemblyStation.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // If close enough, submit component
            if (Vector3.Distance(transform.position, assemblyStation.transform.position) < 1.5f)
            {
                if (ToyManager.Instance.AddComponentToCurrentToy(heldComponent))
                {
                    Debug.Log(characterName + " added " + heldComponent + " to toy!");
                    heldComponent = "";
                }
                else
                {
                    Debug.Log("AI: Wrong component for current toy!");
                }
            }
        }
    }

    void CraftComponent(string componentType)
    {
        WorkStation[] stations = FindObjectsOfType<WorkStation>();
        WorkStation targetStation = null;

        foreach (WorkStation station in stations)
        {
            if (station.componentProduced == componentType && station.IsAvailable())
            {
                targetStation = station;
                break;
            }
        }

        if (targetStation != null)
        {
            // Move towards station
            Vector3 direction = (targetStation.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // If close enough, start interaction
            if (Vector3.Distance(transform.position, targetStation.transform.position) < 1.5f)
            {
                if (targetStation.StartInteraction(transform))
                {
                    currentStation = targetStation;
                    isInteracting = true;
                    Debug.Log(characterName + " started crafting " + componentType);
                }
            }
        }
    }

    public void SetAIControlled(bool controlled)
    {
        isAIControlled = controlled;
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

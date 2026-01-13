using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AICompanion : MonoBehaviour
{
    [Header("References")]
    public Transform leftHand;
    private RecipeManager recipeManager;

    [Header("Settings")]
    public float interactRange = 3.5f;
    public float moveSpeed = 5f;
    public float checkInterval = 2f;
    public float stepTimeout = 20f;
    public float stuckThreshold = 0.3f;

    [Header("Agent Mode")]
    public AgentMode currentMode = AgentMode.Manual;

    public enum AgentMode { PLAOnly, LeatherOnly, Manual }

    private NavMeshAgent agent;
    private GameObject heldItem;
    private bool holdingPLA = false;
    private bool holdingLeather = false;
    private bool holdingToy = false;
    private ItemData.ItemType heldPLAType;
    private LeatherData.LeatherType heldLeatherType;

    private enum State { Idle, Working }
    private State currentState = State.Idle;
    private float idleTimer = 0f;
    private float stepTimer = 0f;

    private string currentRecipeName = "";
    private int currentStep = 0;

    public int selectedRecipeIndex = -1;
    private string manuallyAssignedRecipe = "";

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private bool isGoingToTrash = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = interactRange - 0.5f;
        agent.autoBraking = true;

        recipeManager = FindObjectOfType<RecipeManager>();
        if (recipeManager == null)
        {
            enabled = false;
            return;
        }

        if (leftHand == null)
        {
            GameObject hand = new GameObject("LeftHand");
            hand.transform.SetParent(transform);
            hand.transform.localPosition = new Vector3(-0.3f, 0.5f, 0.3f);
            leftHand = hand.transform;
        }

        currentState = State.Idle;
        idleTimer = checkInterval;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        HandleModeSwitching();
        HandleManualAssignmentInput();

        if (currentState == State.Working && heldItem != null) DetectStuck();

        if (isGoingToTrash)
        {
            HandleTrashItem();
            return;
        }

        if (currentState == State.Idle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                idleTimer = checkInterval;
                TryStartNewTask();
            }
        }
        else if (currentState == State.Working)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer > stepTimeout)
            {
                if (heldItem != null)
                {
                    isGoingToTrash = true;
                    return;
                }

                ResetTask();
                return;
            }

            UpdateCurrentTask();
        }
    }

    void HandleModeSwitching()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (currentState == State.Working) ResetTask();
            currentMode = AgentMode.PLAOnly;
            manuallyAssignedRecipe = "";
            selectedRecipeIndex = -1;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (currentState == State.Working) ResetTask();
            currentMode = AgentMode.LeatherOnly;
            manuallyAssignedRecipe = "";
            selectedRecipeIndex = -1;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (currentState == State.Working) ResetTask();
            currentMode = AgentMode.Manual;
            manuallyAssignedRecipe = "";
            selectedRecipeIndex = -1;
        }
    }

    void DetectStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < stuckThreshold)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 3f)
            {
                isGoingToTrash = true;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
    }

    void HandleTrashItem()
    {
        GameObject trashcan = FindClosestByTag("Trashcan");

        if (trashcan == null)
        {
            if (heldItem != null) Destroy(heldItem);
            heldItem = null;
            holdingPLA = false;
            holdingLeather = false;
            holdingToy = false;
            isGoingToTrash = false;
            ResetTask();
            return;
        }

        if (IsNear(trashcan))
        {
            agent.isStopped = true;

            if (heldItem != null)
            {
                Destroy(heldItem);
                heldItem = null;
                holdingPLA = false;
                holdingLeather = false;
                holdingToy = false;
            }

            isGoingToTrash = false;
            ResetTask();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(trashcan.transform.position);
        }
    }

    void HandleManualAssignmentInput()
    {
        if (currentMode != AgentMode.Manual) return;
        if (currentState == State.Working) return;

        List<Recipe> availableRecipes = GetActiveRecipes();
        if (availableRecipes.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && availableRecipes.Count >= 1) selectedRecipeIndex = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && availableRecipes.Count >= 2) selectedRecipeIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && availableRecipes.Count >= 3) selectedRecipeIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4) && availableRecipes.Count >= 4) selectedRecipeIndex = 3;

        if (Input.GetKeyDown(KeyCode.K) && selectedRecipeIndex >= 0 && selectedRecipeIndex < availableRecipes.Count)
        {
            manuallyAssignedRecipe = availableRecipes[selectedRecipeIndex].toyName;
            currentRecipeName = manuallyAssignedRecipe;
            currentStep = 0;
            stepTimer = 0f;
            currentState = State.Working;
            selectedRecipeIndex = -1;
        }
    }

    void TryStartNewTask()
    {
        List<Recipe> recipes = GetFilteredRecipes();
        if (recipes.Count == 0) return;

        Recipe recipe = recipes[Random.Range(0, recipes.Count)];
        currentRecipeName = recipe.toyName;
        currentStep = 0;
        stepTimer = 0f;
        currentState = State.Working;
    }

    List<Recipe> GetFilteredRecipes()
    {
        List<Recipe> allRecipes = GetActiveRecipes();
        List<Recipe> filtered = new List<Recipe>();

        foreach (Recipe recipe in allRecipes)
        {
            string lower = recipe.toyName.ToLower();

            switch (currentMode)
            {
                case AgentMode.PLAOnly:
                    if (IsPrinterTask(lower)) filtered.Add(recipe);
                    break;

                case AgentMode.LeatherOnly:
                    if (!IsPrinterTask(lower)) filtered.Add(recipe);
                    break;

                case AgentMode.Manual:
                    if (!string.IsNullOrEmpty(manuallyAssignedRecipe) &&
                        recipe.toyName.Equals(manuallyAssignedRecipe, System.StringComparison.OrdinalIgnoreCase))
                    {
                        filtered.Add(recipe);
                    }
                    break;
            }
        }

        return filtered;
    }

    bool IsPrinterTask(string recipeName)
    {
        string lower = recipeName.ToLower();
        return lower.Contains("blue") || lower.Contains("yellow") || lower.Contains("green") ||
               lower.Contains("toy") || lower.Contains("boat") || lower.Contains("steamroller") ||
               lower.Contains("brick");
    }

    void UpdateCurrentTask()
    {
        if (!IsRecipeActive(currentRecipeName))
        {
            if (currentMode == AgentMode.Manual) manuallyAssignedRecipe = "";
            ResetTask();
            return;
        }

        if (IsPrinterTask(currentRecipeName.ToLower())) ExecutePrinterTask();
        else ExecuteCutterTask();
    }

    void ExecutePrinterTask()
    {
        switch (currentStep)
        {
            case 0:
                if (holdingPLA)
                {
                    currentStep = 1;
                    stepTimer = 0f;
                    return;
                }

                ItemData.ItemType plaType = GetPLAType(currentRecipeName);
                GameObject pla = FindItemByType(plaType);

                if (pla == null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(pla))
                {
                    agent.isStopped = true;
                    PickUpPLA(pla);
                    currentStep = 1;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(pla.transform.position);
                }
                break;

            case 1:
                if (!holdingPLA)
                {
                    currentStep = 0;
                    return;
                }

                GameObject printer = FindClosestByTag("Printer");
                if (printer == null)
                {
                    ResetTask();
                    return;
                }

                PrinterLogic printerCheck = printer.GetComponent<PrinterLogic>();
                if (printerCheck != null && printerCheck.GetLoadedPLAType() != null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(printer))
                {
                    agent.isStopped = true;
                    PrinterLogic logic = printer.GetComponent<PrinterLogic>();
                    if (logic != null)
                    {
                        logic.ProcessItem(heldPLAType);
                        Destroy(heldItem);
                        heldItem = null;
                        holdingPLA = false;
                        currentStep = 2;
                        stepTimer = 0f;
                        StartCoroutine(WaitThenContinue(0.6f));
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(printer.transform.position);
                }
                break;

            case 2:
                GameObject printerToStart = FindClosestByTag("Printer");
                if (printerToStart == null)
                {
                    ResetTask();
                    return;
                }

                PrinterLogic startLogic = printerToStart.GetComponent<PrinterLogic>();
                if (startLogic == null || !startLogic.CanBake()) return;

                if (IsNear(printerToStart))
                {
                    agent.isStopped = true;
                    if (startLogic.CanBake())
                    {
                        startLogic.StartCoroutine("BakeItem");
                        currentStep = 3;
                        stepTimer = 0f;
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(printerToStart.transform.position);
                }
                break;

            case 3:
                GameObject finished = FindClosestByTag("FinishedPrinter");
                if (finished != null)
                {
                    currentStep = 4;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = true;
                }
                break;

            case 4:
                if (holdingToy)
                {
                    currentStep = 5;
                    stepTimer = 0f;
                    return;
                }

                GameObject finishedPrinter = FindClosestByTag("FinishedPrinter");
                if (finishedPrinter == null)
                {
                    currentStep = 3;
                    return;
                }

                if (IsNear(finishedPrinter))
                {
                    agent.isStopped = true;
                    PickUpToyFromPrinter(finishedPrinter);
                    currentStep = 5;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(finishedPrinter.transform.position);
                }
                break;

            case 5:
                if (!holdingToy)
                {
                    ResetTask();
                    return;
                }

                GameObject giftbox = FindClosestByTag("Giftbox");
                if (giftbox == null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(giftbox))
                {
                    agent.isStopped = true;
                    DeliverToy();
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(giftbox.transform.position);
                }
                break;
        }
    }

    void ExecuteCutterTask()
    {
        switch (currentStep)
        {
            case 0:
                if (holdingLeather)
                {
                    currentStep = 1;
                    stepTimer = 0f;
                    return;
                }

                var types = GetLeatherTypes(currentRecipeName);
                GameObject leather1 = FindLeatherByType(types.Item1);

                if (leather1 == null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(leather1))
                {
                    agent.isStopped = true;
                    PickUpLeather(leather1);
                    currentStep = 1;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(leather1.transform.position);
                }
                break;

            case 1:
                if (!holdingLeather)
                {
                    currentStep = 0;
                    return;
                }

                GameObject cutter1 = FindAvailableCutter();
                if (cutter1 == null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(cutter1))
                {
                    agent.isStopped = true;
                    LaserCutterLogic logic = cutter1.GetComponent<LaserCutterLogic>();

                    if (logic != null && logic.CanLoadLeather())
                    {
                        logic.LoadLeather(heldLeatherType);
                        Destroy(heldItem);
                        heldItem = null;
                        holdingLeather = false;
                        currentStep = 2;
                        stepTimer = 0f;
                    }
                    else
                    {
                        ResetTask();
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(cutter1.transform.position);
                }
                break;

            case 2:
                if (holdingLeather)
                {
                    currentStep = 3;
                    stepTimer = 0f;
                    return;
                }

                var types2 = GetLeatherTypes(currentRecipeName);
                GameObject leather2 = FindLeatherByType(types2.Item2);

                if (leather2 == null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(leather2))
                {
                    agent.isStopped = true;
                    PickUpLeather(leather2);
                    currentStep = 3;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(leather2.transform.position);
                }
                break;

            case 3:
                if (!holdingLeather)
                {
                    currentStep = 2;
                    return;
                }

                GameObject cutter2 = FindCutterWithOneLeather();
                if (cutter2 == null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(cutter2))
                {
                    agent.isStopped = true;
                    LaserCutterLogic logic = cutter2.GetComponent<LaserCutterLogic>();

                    if (logic != null && logic.CanLoadLeather())
                    {
                        logic.LoadLeather(heldLeatherType);
                        Destroy(heldItem);
                        heldItem = null;
                        holdingLeather = false;

                        if (logic.CanStartCutting())
                        {
                            logic.StartCutting();
                            currentStep = 4;
                            stepTimer = 0f;
                        }
                        else
                        {
                            ResetTask();
                        }
                    }
                    else
                    {
                        ResetTask();
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(cutter2.transform.position);
                }
                break;

            case 4:
                GameObject cutter3 = FindClosestByTag("LaserCutter");
                if (cutter3 != null)
                {
                    LaserCutterLogic logic = cutter3.GetComponent<LaserCutterLogic>();
                    if (logic != null && logic.IsFinished())
                    {
                        currentStep = 5;
                        stepTimer = 0f;
                    }
                    else
                    {
                        agent.isStopped = true;
                    }
                }
                else
                {
                    agent.isStopped = true;
                }
                break;

            case 5:
                if (holdingToy)
                {
                    currentStep = 6;
                    stepTimer = 0f;
                    return;
                }

                GameObject finishedCutter = FindClosestByTag("LaserCutter");
                if (finishedCutter == null)
                {
                    ResetTask();
                    return;
                }

                LaserCutterLogic cutterLogic = finishedCutter.GetComponent<LaserCutterLogic>();
                if (cutterLogic == null || !cutterLogic.IsFinished())
                {
                    currentStep = 4;
                    return;
                }

                if (IsNear(finishedCutter))
                {
                    agent.isStopped = true;
                    PickUpCutterItem(finishedCutter);
                    currentStep = 6;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(finishedCutter.transform.position);
                }
                break;

            case 6:
                if (!holdingToy)
                {
                    ResetTask();
                    return;
                }

                GameObject giftbox = FindClosestByTag("Giftbox");
                if (giftbox == null)
                {
                    ResetTask();
                    return;
                }

                if (IsNear(giftbox))
                {
                    agent.isStopped = true;
                    DeliverToy();
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(giftbox.transform.position);
                }
                break;
        }
    }

    bool IsNear(GameObject target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.transform.position) <= interactRange;
    }

    System.Collections.IEnumerator WaitThenContinue(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    void ResetTask()
    {
        currentState = State.Idle;
        idleTimer = checkInterval;
        currentRecipeName = "";
        currentStep = 0;
        stepTimer = 0f;
        stuckTimer = 0f;
        isGoingToTrash = false;
        agent.isStopped = true;
        agent.ResetPath();
    }

    List<Recipe> GetActiveRecipes()
    {
        List<Recipe> recipes = new List<Recipe>();
        foreach (Transform child in recipeManager.ticketTray)
        {
            Recipe recipe = child.GetComponent<Recipe>();
            if (recipe != null) recipes.Add(recipe);
        }
        return recipes;
    }

    bool IsRecipeActive(string recipeName)
    {
        foreach (Transform child in recipeManager.ticketTray)
        {
            Recipe recipe = child.GetComponent<Recipe>();
            if (recipe != null && recipe.toyName.Equals(recipeName, System.StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    ItemData.ItemType GetPLAType(string recipeName)
    {
        string lower = recipeName.ToLower();
        if (lower.Contains("blue") || lower.Contains("boat")) return ItemData.ItemType.BluePLA;
        if (lower.Contains("yellow") || lower.Contains("steamroller")) return ItemData.ItemType.YellowPLA;
        if (lower.Contains("green") || lower.Contains("brick")) return ItemData.ItemType.GreenPLA;
        return ItemData.ItemType.BluePLA;
    }

    (LeatherData.LeatherType, LeatherData.LeatherType) GetLeatherTypes(string recipeName)
    {
        string lower = recipeName.ToLower();
        if (lower.Contains("football")) return (LeatherData.LeatherType.Brown, LeatherData.LeatherType.Brown);
        if (lower.Contains("hat")) return (LeatherData.LeatherType.Silver, LeatherData.LeatherType.Purple);
        if (lower.Contains("backpack")) return (LeatherData.LeatherType.Silver, LeatherData.LeatherType.Yellow);
        return (LeatherData.LeatherType.Brown, LeatherData.LeatherType.Brown);
    }

    GameObject FindItemByType(ItemData.ItemType type)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("PLA");
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var item in items)
        {
            ItemData data = item.GetComponent<ItemData>();
            if (data != null && data.itemType == type)
            {
                float dist = Vector3.Distance(transform.position, item.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = item;
                }
            }
        }
        return closest;
    }

    GameObject FindLeatherByType(LeatherData.LeatherType type)
    {
        GameObject[] leathers = GameObject.FindGameObjectsWithTag("Leather");
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var leather in leathers)
        {
            LeatherData data = leather.GetComponent<LeatherData>();
            if (data != null && data.leatherType == type)
            {
                float dist = Vector3.Distance(transform.position, leather.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = leather;
                }
            }
        }
        return closest;
    }

    GameObject FindClosestByTag(string tag)
    {
        try
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            GameObject closest = null;
            float minDist = float.MaxValue;

            foreach (var obj in objects)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = obj;
                }
            }
            return closest;
        }
        catch { return null; }
    }

    GameObject FindAvailableCutter()
    {
        try
        {
            GameObject[] cutters = GameObject.FindGameObjectsWithTag("LaserCutter");
            GameObject closest = null;
            float minDist = float.MaxValue;

            foreach (var cutter in cutters)
            {
                LaserCutterLogic logic = cutter.GetComponent<LaserCutterLogic>();
                if (logic != null && logic.CanLoadLeather() && !logic.IsCutting() && !logic.IsFinished())
                {
                    float dist = Vector3.Distance(transform.position, cutter.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = cutter;
                    }
                }
            }
            return closest;
        }
        catch { return null; }
    }

    GameObject FindCutterWithOneLeather()
    {
        try
        {
            GameObject[] cutters = GameObject.FindGameObjectsWithTag("LaserCutter");
            GameObject closest = null;
            float minDist = float.MaxValue;

            foreach (var cutter in cutters)
            {
                LaserCutterLogic logic = cutter.GetComponent<LaserCutterLogic>();
                if (logic != null && logic.CanLoadLeather() && !logic.IsCutting() && !logic.IsFinished())
                {
                    float dist = Vector3.Distance(transform.position, cutter.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = cutter;
                    }
                }
            }
            return closest;
        }
        catch { return null; }
    }

    void PickUpPLA(GameObject plaObject)
    {
        ItemData data = plaObject.GetComponent<ItemData>();
        if (data == null) return;

        heldPLAType = data.itemType;
        holdingPLA = true;
        holdingToy = false;
        holdingLeather = false;

        GameObject prefab = data.itemPrefab != null ? data.itemPrefab : plaObject;
        heldItem = Instantiate(prefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.tag = "Untagged";

        DisablePhysics(heldItem);
    }

    void PickUpLeather(GameObject leatherObject)
    {
        LeatherData data = leatherObject.GetComponent<LeatherData>();
        if (data == null) return;

        heldLeatherType = data.leatherType;
        holdingLeather = true;
        holdingToy = false;
        holdingPLA = false;

        GameObject prefab = data.leatherPrefab != null ? data.leatherPrefab : leatherObject;
        heldItem = Instantiate(prefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.tag = "Untagged";

        DisablePhysics(heldItem);
    }

    void PickUpToyFromPrinter(GameObject printer)
    {
        PrinterLogic logic = printer.GetComponent<PrinterLogic>();
        if (logic == null) return;

        GameObject toyPrefab = logic.GetToyPrefab();
        if (toyPrefab == null) return;

        if (logic.GetLoadedPLAType().HasValue) heldPLAType = logic.GetLoadedPLAType().Value;

        heldItem = Instantiate(toyPrefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.tag = "Untagged";
        holdingToy = true;
        holdingPLA = false;
        holdingLeather = false;

        if (logic.IsFried())
        {
            FriedToyMarker marker = heldItem.GetComponent<FriedToyMarker>();
            if (marker == null) marker = heldItem.AddComponent<FriedToyMarker>();
            marker.isFried = true;
        }

        heldItem.transform.localScale = heldPLAType == ItemData.ItemType.GreenPLA ? Vector3.one * 0.3f : Vector3.one * 2.5f;
        heldItem.transform.localRotation = Quaternion.identity;

        Collider col = heldItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        logic.ClearAfterPickup();
    }

    void PickUpCutterItem(GameObject cutter)
    {
        LaserCutterLogic logic = cutter.GetComponent<LaserCutterLogic>();
        if (logic == null || !logic.IsFinished()) return;

        GameObject itemPrefab = logic.GetFinishedItem();
        if (itemPrefab == null) return;

        heldItem = Instantiate(itemPrefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.tag = "Untagged";
        holdingToy = true;
        holdingPLA = false;
        holdingLeather = false;

        heldItem.transform.localScale = Vector3.one * 2.0f;
        heldItem.transform.localRotation = Quaternion.identity;

        DisablePhysics(heldItem);
        logic.ClearAfterPickup();
    }

    void DeliverToy()
    {
        if (!holdingToy || heldItem == null) return;

        if (recipeManager != null && recipeManager.HasMatchingRecipe(currentRecipeName))
        {
            int points = recipeManager.CompleteRecipe(currentRecipeName);

            Points pointsSystem = FindObjectOfType<Points>();
            if (pointsSystem != null) pointsSystem.AddPoints(points);

            Quota quota = FindObjectOfType<Quota>();
            if (quota != null) quota.QuotaProgressOne(1);

            Destroy(heldItem);
            heldItem = null;
            holdingToy = false;

            if (currentMode == AgentMode.Manual) manuallyAssignedRecipe = "";

            currentState = State.Idle;
            idleTimer = checkInterval;
            currentRecipeName = "";
            currentStep = 0;
            stepTimer = 0f;
        }
        else
        {
            ResetTask();
        }
    }

    void DisablePhysics(GameObject obj)
    {
        if (obj.TryGetComponent<Collider>(out var c)) c.enabled = false;
        if (obj.TryGetComponent<Rigidbody>(out var r))
        {
            r.isKinematic = true;
            r.detectCollisions = false;
        }
    }
}
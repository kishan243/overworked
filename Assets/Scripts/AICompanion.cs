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
    public float stepTimeout = 15f;

    [Header("Agent Mode")]
    public AgentMode currentMode = AgentMode.Manual;

    public enum AgentMode
    {
        PLAOnly,      // Only does PLA/printer toys
        LeatherOnly,  // Only does leather/cutter toys
        Manual        // User assigns specific recipe
    }

    private NavMeshAgent agent;
    private GameObject heldItem;
    private bool holdingPLA = false;
    private bool holdingLeather = false;
    private bool holdingToy = false;
    private ItemData.ItemType heldPLAType;
    private LeatherData.LeatherType heldLeatherType;

    // State machine
    private enum State { Idle, Working }
    private State currentState = State.Idle;
    private float idleTimer = 0f;
    private float stepTimer = 0f;

    // Current task
    private string currentRecipeName = "";
    private int currentStep = 0;

    // Manual assignment
    private int selectedRecipeIndex = -1;
    private string manuallyAssignedRecipe = "";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("❌ AICompanion: No NavMeshAgent!");
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = interactRange - 0.5f;
        agent.autoBraking = true;

        recipeManager = FindObjectOfType<RecipeManager>();
        if (recipeManager == null)
        {
            Debug.LogError("❌ AICompanion: No RecipeManager!");
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

        Debug.Log($"🤖 AI Companion Ready! Mode: {currentMode}");
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        // Handle manual assignment input
        HandleManualAssignmentInput();

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
                Debug.LogWarning($"⏱️ AI timeout on step {currentStep}");
                ResetTask();
                return;
            }

            UpdateCurrentTask();
        }
    }

    void HandleManualAssignmentInput()
    {
        if (currentMode != AgentMode.Manual) return;
        if (currentState == State.Working) return; // Don't allow changes while working

        List<Recipe> availableRecipes = GetActiveRecipes();
        if (availableRecipes.Count == 0) return;

        // Select recipe with 1, 2, 3
        if (Input.GetKeyDown(KeyCode.Alpha1) && availableRecipes.Count >= 1)
        {
            selectedRecipeIndex = 0;
            Debug.Log($"📋 Selected Recipe 1: {availableRecipes[0].toyName}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && availableRecipes.Count >= 2)
        {
            selectedRecipeIndex = 1;
            Debug.Log($"📋 Selected Recipe 2: {availableRecipes[1].toyName}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && availableRecipes.Count >= 3)
        {
            selectedRecipeIndex = 2;
            Debug.Log($"📋 Selected Recipe 3: {availableRecipes[2].toyName}");
        }

        // Assign with K
        if (Input.GetKeyDown(KeyCode.K) && selectedRecipeIndex >= 0 && selectedRecipeIndex < availableRecipes.Count)
        {
            manuallyAssignedRecipe = availableRecipes[selectedRecipeIndex].toyName;
            Debug.Log($"✅ Assigned AI to: {manuallyAssignedRecipe}");

            // Immediately start working on it
            currentRecipeName = manuallyAssignedRecipe;
            currentStep = 0;
            stepTimer = 0f;
            currentState = State.Working;
            selectedRecipeIndex = -1; // Reset selection
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

        Debug.Log($"🎯 AI Starting: {currentRecipeName} (Mode: {currentMode})");
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
                    // Only PLA/printer tasks
                    if (IsPrinterTask(lower))
                        filtered.Add(recipe);
                    break;

                case AgentMode.LeatherOnly:
                    // Only leather/cutter tasks
                    if (!IsPrinterTask(lower))
                        filtered.Add(recipe);
                    break;

                case AgentMode.Manual:
                    // Only work on manually assigned recipe
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
            Debug.Log($"⚠️ Recipe {currentRecipeName} completed by someone else");

            // If manual mode, clear the assignment
            if (currentMode == AgentMode.Manual)
            {
                manuallyAssignedRecipe = "";
            }

            ResetTask();
            return;
        }

        string lower = currentRecipeName.ToLower();

        if (IsPrinterTask(lower))
        {
            ExecutePrinterTask();
        }
        else
        {
            ExecuteCutterTask();
        }
    }

    // ==================== PRINTER TASK ====================
    void ExecutePrinterTask()
    {
        switch (currentStep)
        {
            case 0: // Pick up PLA
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
                    Debug.Log($"❌ No {plaType} found on tables");
                    ResetTask();
                    return;
                }

                if (IsNear(pla))
                {
                    agent.isStopped = true;
                    PickUpPLA(pla);
                    Debug.Log($"✓ AI picked up {plaType}");
                    currentStep = 1;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(pla.transform.position);
                }
                break;

            case 1: // Load PLA into printer
                if (!holdingPLA)
                {
                    Debug.Log("⚠️ Lost PLA, restarting");
                    currentStep = 0;
                    return;
                }

                GameObject printer = FindClosestByTag("Printer");
                if (printer == null)
                {
                    Debug.Log("❌ No available printer found");
                    ResetTask();
                    return;
                }

                // Check if this printer is actually available (not loaded)
                PrinterLogic printerCheck = printer.GetComponent<PrinterLogic>();
                if (printerCheck != null && printerCheck.GetLoadedPLAType() != null)
                {
                    Debug.Log("⚠️ Printer already loaded, looking for another");
                    ResetTask();
                    return;
                }

                if (IsNear(printer))
                {
                    agent.isStopped = true;
                    PrinterLogic logic = printer.GetComponent<PrinterLogic>();
                    if (logic != null)
                    {
                        // Load the PLA (this will transform the printer)
                        logic.ProcessItem(heldPLAType);

                        // Destroy only the visual copy in AI's hand
                        Destroy(heldItem);
                        heldItem = null;
                        holdingPLA = false;

                        Debug.Log("✓ AI loaded PLA into printer");
                        currentStep = 2;
                        stepTimer = 0f;

                        // Wait for printer to transform
                        StartCoroutine(WaitThenContinue(0.6f));
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(printer.transform.position);
                }
                break;

            case 2: // Start printer
                GameObject printerToStart = FindClosestByTag("Printer");
                if (printerToStart == null)
                {
                    Debug.Log("⚠️ Can't find loaded printer");
                    ResetTask();
                    return;
                }

                // Make sure this printer can actually bake
                PrinterLogic startLogic = printerToStart.GetComponent<PrinterLogic>();
                if (startLogic == null || !startLogic.CanBake())
                {
                    Debug.Log("⚠️ Printer not ready to bake");
                    return; // Keep waiting
                }

                if (IsNear(printerToStart))
                {
                    agent.isStopped = true;

                    if (startLogic.CanBake())
                    {
                        startLogic.StartCoroutine("BakeItem");
                        Debug.Log("✓ AI started printer");
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

            case 3: // Wait for printer to finish
                GameObject finished = FindClosestByTag("FinishedPrinter");
                if (finished != null)
                {
                    Debug.Log("✓ Printer finished baking!");
                    currentStep = 4;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = true;
                }
                break;

            case 4: // Pick up toy
                if (holdingToy)
                {
                    currentStep = 5;
                    stepTimer = 0f;
                    return;
                }

                GameObject finishedPrinter = FindClosestByTag("FinishedPrinter");
                if (finishedPrinter == null)
                {
                    Debug.Log("⚠️ Finished printer disappeared");
                    currentStep = 3;
                    return;
                }

                if (IsNear(finishedPrinter))
                {
                    agent.isStopped = true;
                    PickUpToyFromPrinter(finishedPrinter);
                    Debug.Log("✓ AI picked up toy from printer");
                    currentStep = 5;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(finishedPrinter.transform.position);
                }
                break;

            case 5: // Deliver to giftbox
                if (!holdingToy)
                {
                    Debug.Log("⚠️ Lost toy!");
                    ResetTask();
                    return;
                }

                GameObject giftbox = FindClosestByTag("Giftbox");
                if (giftbox == null)
                {
                    Debug.Log("❌ No giftbox found");
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

    // ==================== CUTTER TASK ====================
    void ExecuteCutterTask()
    {
        switch (currentStep)
        {
            case 0: // Pick up first leather
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
                    Debug.Log($"❌ No {types.Item1} leather found");
                    ResetTask();
                    return;
                }

                if (IsNear(leather1))
                {
                    agent.isStopped = true;
                    PickUpLeather(leather1);
                    Debug.Log($"✓ AI picked up first leather: {types.Item1}");
                    currentStep = 1;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(leather1.transform.position);
                }
                break;

            case 1: // Load first leather into cutter
                if (!holdingLeather)
                {
                    Debug.Log("⚠️ Lost leather, restarting");
                    currentStep = 0;
                    return;
                }

                GameObject cutter1 = FindAvailableCutter();
                if (cutter1 == null)
                {
                    Debug.Log("❌ No available laser cutter found");
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

                        // Destroy only the visual copy in AI's hand
                        Destroy(heldItem);
                        heldItem = null;
                        holdingLeather = false;

                        Debug.Log($"✓ AI loaded first leather into cutter");
                        currentStep = 2;
                        stepTimer = 0f;
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Cutter can't accept leather - might be full!");
                        ResetTask();
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(cutter1.transform.position);
                }
                break;

            case 2: // Pick up second leather
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
                    Debug.Log($"❌ No {types2.Item2} leather found");
                    ResetTask();
                    return;
                }

                if (IsNear(leather2))
                {
                    agent.isStopped = true;
                    PickUpLeather(leather2);
                    Debug.Log($"✓ AI picked up second leather: {types2.Item2}");
                    currentStep = 3;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(leather2.transform.position);
                }
                break;

            case 3: // Load second leather and start cutting
                if (!holdingLeather)
                {
                    Debug.Log("⚠️ Lost second leather, going back");
                    currentStep = 2;
                    return;
                }

                GameObject cutter2 = FindCutterWithOneLeather();
                if (cutter2 == null)
                {
                    Debug.Log("❌ No cutter found with first leather loaded");
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

                        // Destroy only the visual copy in AI's hand
                        Destroy(heldItem);
                        heldItem = null;
                        holdingLeather = false;

                        Debug.Log($"✓ AI loaded second leather");

                        // Now start cutting if possible
                        if (logic.CanStartCutting())
                        {
                            logic.StartCutting();
                            Debug.Log("✓ AI started cutting!");
                            currentStep = 4;
                            stepTimer = 0f;
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ Can't start cutting - recipe mismatch?");
                            ResetTask();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Cutter already has 2 items or can't accept more!");
                        ResetTask();
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(cutter2.transform.position);
                }
                break;

            case 4: // Wait for cutting to finish
                GameObject cutter3 = FindClosestByTag("LaserCutter");
                if (cutter3 != null)
                {
                    LaserCutterLogic logic = cutter3.GetComponent<LaserCutterLogic>();
                    if (logic != null && logic.IsFinished())
                    {
                        Debug.Log("✓ Cutting finished!");
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

            case 5: // Pick up finished item
                if (holdingToy)
                {
                    currentStep = 6;
                    stepTimer = 0f;
                    return;
                }

                GameObject finishedCutter = FindClosestByTag("LaserCutter");
                if (finishedCutter == null)
                {
                    Debug.Log("❌ Can't find cutter");
                    ResetTask();
                    return;
                }

                LaserCutterLogic cutterLogic = finishedCutter.GetComponent<LaserCutterLogic>();
                if (cutterLogic == null || !cutterLogic.IsFinished())
                {
                    Debug.Log("⚠️ Cutter not finished yet");
                    currentStep = 4;
                    return;
                }

                if (IsNear(finishedCutter))
                {
                    agent.isStopped = true;
                    PickUpCutterItem(finishedCutter);
                    Debug.Log("✓ AI picked up finished item");
                    currentStep = 6;
                    stepTimer = 0f;
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(finishedCutter.transform.position);
                }
                break;

            case 6: // Deliver to giftbox
                if (!holdingToy)
                {
                    Debug.Log("⚠️ Lost toy!");
                    ResetTask();
                    return;
                }

                GameObject giftbox = FindClosestByTag("Giftbox");
                if (giftbox == null)
                {
                    Debug.Log("❌ No giftbox found");
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

    // ==================== HELPERS ====================
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
        agent.isStopped = true;
        agent.ResetPath();

        // In manual mode, keep the assignment until user changes it
        // In auto modes, this gets cleared naturally
    }

    // ==================== RECIPE HELPERS ====================
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
            if (recipe != null && recipe.toyName.Equals(recipeName, System.StringComparison.OrdinalIgnoreCase))
                return true;
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
        if (lower.Contains("football"))
            return (LeatherData.LeatherType.Brown, LeatherData.LeatherType.Brown);
        if (lower.Contains("hat"))
            return (LeatherData.LeatherType.Silver, LeatherData.LeatherType.Purple);
        if (lower.Contains("backpack"))
            return (LeatherData.LeatherType.Silver, LeatherData.LeatherType.Yellow);

        Debug.LogWarning($"Unknown leather recipe: {recipeName}");
        return (LeatherData.LeatherType.Brown, LeatherData.LeatherType.Brown);
    }

    // ==================== FIND OBJECTS ====================
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

                // Only consider cutters that can load leather (empty or has 1 slot free)
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

                // Find a cutter that can still load leather (meaning it has exactly 1 leather)
                // and is not currently cutting or finished
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

    // ==================== PICKUP FUNCTIONS ====================
    void PickUpPLA(GameObject plaObject)
    {
        ItemData data = plaObject.GetComponent<ItemData>();
        if (data == null) return;

        heldPLAType = data.itemType;
        holdingPLA = true;
        holdingToy = false;
        holdingLeather = false;

        // Just spawn a copy - NEVER destroy the source table item!
        GameObject prefab = data.itemPrefab != null ? data.itemPrefab : plaObject;
        heldItem = Instantiate(prefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.tag = "Untagged";

        DisablePhysics(heldItem);
        // NO DESTROYING - table items stay forever!
    }

    void PickUpLeather(GameObject leatherObject)
    {
        LeatherData data = leatherObject.GetComponent<LeatherData>();
        if (data == null) return;

        heldLeatherType = data.leatherType;
        holdingLeather = true;
        holdingToy = false;
        holdingPLA = false;

        // Just spawn a copy - NEVER destroy the source table item!
        GameObject prefab = data.leatherPrefab != null ? data.leatherPrefab : leatherObject;
        heldItem = Instantiate(prefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.tag = "Untagged";

        DisablePhysics(heldItem);
        // NO DESTROYING - table items stay forever!
    }

    void PickUpToyFromPrinter(GameObject printer)
    {
        PrinterLogic logic = printer.GetComponent<PrinterLogic>();
        if (logic == null)
        {
            Debug.LogError("❌ AI: Printer has no PrinterLogic!");
            return;
        }

        GameObject toyPrefab = logic.GetToyPrefab();
        if (toyPrefab == null)
        {
            Debug.LogError("❌ AI: Printer has no toy prefab!");
            return;
        }

        // Store the PLA type if available
        if (logic.GetLoadedPLAType().HasValue)
        {
            heldPLAType = logic.GetLoadedPLAType().Value;
        }

        // Create the toy in AI's hand
        heldItem = Instantiate(toyPrefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.tag = "Untagged";
        holdingToy = true;
        holdingPLA = false;
        holdingLeather = false;

        // Check if toy is fried
        if (logic.IsFried())
        {
            FriedToyMarker marker = heldItem.GetComponent<FriedToyMarker>();
            if (marker == null)
            {
                marker = heldItem.AddComponent<FriedToyMarker>();
            }
            marker.isFried = true;
            Debug.Log("🔥 AI picked up FRIED toy!");
        }

        // Scale based on PLA type (GreenPLA toys are smaller)
        if (heldPLAType == ItemData.ItemType.GreenPLA)
        {
            heldItem.transform.localScale = Vector3.one * 0.3f;
        }
        else
        {
            heldItem.transform.localScale = Vector3.one * 2.5f;
        }

        heldItem.transform.localRotation = Quaternion.identity;

        // Disable physics
        Collider col = heldItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Clear the printer
        logic.ClearAfterPickup();

        Debug.Log($"✓ AI successfully picked up toy from printer");
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

            Debug.Log($"🎁 AI Delivered {currentRecipeName} for {points} points!");

            Destroy(heldItem);
            heldItem = null;
            holdingToy = false;

            // Clear manual assignment after completion
            if (currentMode == AgentMode.Manual)
            {
                manuallyAssignedRecipe = "";
            }

            currentState = State.Idle;
            idleTimer = checkInterval;
            currentRecipeName = "";
            currentStep = 0;
            stepTimer = 0f;
        }
        else
        {
            Debug.LogWarning($"⚠️ No matching recipe!");
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
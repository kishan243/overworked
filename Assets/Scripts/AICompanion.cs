using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class AICompanion : MonoBehaviour
{
    [Header("References")]
    public Transform leftHand;
    private RecipeManager recipeManager;
    private Timer gameTimer;
    private Quota quota;

    [Header("Settings")]
    public float interactRange = 3.5f;
    public float moveSpeed = 8f;
    public float recipeCheckInterval = 3f;

    private NavMeshAgent agent;

    // FSM States
    private enum State
    {
        Idle,
        DeterminingTask,
        ExecutingTask,
        FollowingCommand
    }
    private State currentState = State.Idle;

    // Task types - now recipe-aware
    private enum TaskType
    {
        None,
        BuildBlueToy,       // Blue PLA -> Printer -> Blue Toy
        BuildYellowToy,     // Yellow PLA -> Printer -> Yellow Toy
        BuildGreenToy,      // Green PLA -> Printer -> Green Toy
        BuildFootball,      // Brown+Purple Leather -> Cutter -> Football
        BuildHat,           // Silver+Yellow Leather -> Cutter -> Hat
        BuildBackpack       // Brown+Silver Leather -> Cutter -> Backpack
    }
    private TaskType currentTask = TaskType.None;
    private string targetRecipeName = "";

    // Task execution state
    private enum ExecutionPhase
    {
        CollectingMaterial1,
        LoadingMachine,
        CollectingMaterial2,    // For leather items
        StartingMachine,
        WaitingForCompletion,
        CollectingFinished,
        DeliveringToGiftbox
    }
    private ExecutionPhase currentPhase = ExecutionPhase.CollectingMaterial1;

    // Item holding
    private GameObject heldItem;
    private ItemData.ItemType heldPLAType;
    private LeatherData.LeatherType heldLeatherType;
    private bool holdingPLA = false;
    private bool holdingLeather = false;
    private bool holdingToy = false;

    // Pathfinding
    private GameObject targetObject;
    private float stateTimer = 0f;
    private float recipeCheckTimer = 0f;

    // MDP - Advanced Task Scoring System
    private Dictionary<TaskType, float> taskScores = new Dictionary<TaskType, float>();

    // N-gram tracking - learns patterns
    private List<TaskType> taskHistory = new List<TaskType>();
    private const int maxHistory = 10;
    private Dictionary<TaskType, int> taskFrequency = new Dictionary<TaskType, int>();

    // Player control
    private bool playerControlled = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("AICompanion: No NavMeshAgent component found!");
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // Find game managers
        recipeManager = FindObjectOfType<RecipeManager>();
        gameTimer = FindObjectOfType<Timer>();
        quota = FindObjectOfType<Quota>();

        if (recipeManager == null) Debug.LogError("RecipeManager not found!");

        if (leftHand == null)
        {
            GameObject hand = new GameObject("LeftHand");
            hand.transform.SetParent(transform);
            hand.transform.localPosition = new Vector3(-0.3f, 0.5f, 0.3f);
            leftHand = hand.transform;
        }

        // Initialize task frequency tracking
        foreach (TaskType task in System.Enum.GetValues(typeof(TaskType)))
        {
            taskFrequency[task] = 0;
        }

        // Ignore collision with player
        StartCoroutine(SetupPlayerCollisionIgnore());

        currentState = State.Idle;
        stateTimer = 2f;

        Debug.Log("🤖 AI Companion initialized - Smart Recipe System Active");
    }

    System.Collections.IEnumerator SetupPlayerCollisionIgnore()
    {
        yield return new WaitForSeconds(0.5f);

        Movement player = FindObjectOfType<Movement>();
        if (player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            Collider aiCollider = GetComponent<Collider>();

            if (playerCollider != null && aiCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, aiCollider, true);
                Debug.Log("✓ Player-AI collision ignored");
            }
        }
    }

    void Update()
    {
        if (agent == null) return;

        // Toggle player control with C
        if (Input.GetKeyDown(KeyCode.C))
        {
            playerControlled = !playerControlled;
            if (playerControlled)
            {
                currentState = State.FollowingCommand;
                Debug.Log("🎮 AI: Player control enabled");
            }
            else
            {
                currentState = State.Idle;
                stateTimer = 0f;
                Debug.Log("🤖 AI: Autonomous mode resumed");
            }
        }

        // Right-click to command AI
        if (playerControlled && Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(hit.point);
                    targetObject = hit.collider.gameObject;
                }
            }
        }

        // FSM Update
        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.DeterminingTask:
                UpdateDeterminingTask();
                break;
            case State.ExecutingTask:
                UpdateExecutingTask();
                break;
            case State.FollowingCommand:
                UpdateFollowingCommand();
                break;
        }
    }

    // ===== FSM STATE: IDLE =====
    void UpdateIdle()
    {
        if (playerControlled) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            stateTimer = recipeCheckInterval;
            currentState = State.DeterminingTask;
        }
    }

    // ===== FSM STATE: DETERMINING TASK (MDP) =====
    void UpdateDeterminingTask()
    {
        if (recipeManager == null)
        {
            Debug.LogWarning("No RecipeManager - AI cannot function");
            currentState = State.Idle;
            return;
        }

        // Get all active recipes
        List<Recipe> activeRecipes = GetActiveRecipes();

        if (activeRecipes.Count == 0)
        {
            Debug.Log("📋 No active recipes - AI idle");
            currentState = State.Idle;
            return;
        }

        // MDP: Calculate scores for each recipe
        CalculateTaskScores(activeRecipes);

        // Choose best task
        currentTask = ChooseBestTask();

        if (currentTask != TaskType.None)
        {
            currentPhase = ExecutionPhase.CollectingMaterial1;
            currentState = State.ExecutingTask;
            Debug.Log($"🎯 AI chose task: {currentTask} for recipe: {targetRecipeName}");
        }
        else
        {
            Debug.Log("❌ No valid tasks available");
            currentState = State.Idle;
        }
    }

    List<Recipe> GetActiveRecipes()
    {
        List<Recipe> recipes = new List<Recipe>();

        foreach (Transform child in recipeManager.ticketTray)
        {
            Recipe recipe = child.GetComponent<Recipe>();
            if (recipe != null)
            {
                recipes.Add(recipe);
            }
        }

        return recipes;
    }

    void CalculateTaskScores(List<Recipe> recipes)
    {
        taskScores.Clear();

        foreach (Recipe recipe in recipes)
        {
            TaskType task = GetTaskTypeFromRecipe(recipe.toyName);
            if (task == TaskType.None) continue;

            float score = 0f;

            // ===== MDP FACTOR 1: Time Urgency =====
            float timeElapsed = Time.time - recipe.spawnTime;
            float urgency = Mathf.Clamp01(timeElapsed / recipe.maxTime);
            float timeScore = urgency * 100f; // Higher score for older recipes
            score += timeScore;

            // ===== MDP FACTOR 2: Point Value =====
            int pointValue = recipe.GetPointsForCompletion();
            float pointScore = pointValue / 100f * 50f; // Normalize to 0-50
            score += pointScore;

            // ===== MDP FACTOR 3: Game State - Time Pressure =====
            if (gameTimer != null && gameTimer.timeRemaining < 30f)
            {
                // Low time = prioritize ANY recipe completion
                score *= 1.5f;
            }

            // ===== MDP FACTOR 4: Quota Progress =====
            if (quota != null)
            {
                // If quota is far from goal, prioritize faster tasks
                float quotaProgress = 0f; // You'd get this from quota
                if (quotaProgress < 0.5f)
                {
                    // Prioritize simple printer tasks (faster)
                    if (task == TaskType.BuildBlueToy || task == TaskType.BuildYellowToy || task == TaskType.BuildGreenToy)
                    {
                        score *= 1.2f;
                    }
                }
            }

            // ===== N-GRAM FACTOR: Task History Learning =====
            // Penalize recently done tasks to create variety
            if (taskHistory.Count > 0)
            {
                int recentCount = taskHistory.Take(5).Count(t => t == task);
                if (recentCount > 0)
                {
                    score *= (1f - (recentCount * 0.15f)); // Up to -45% for repeated tasks
                }
            }

            // ===== N-GRAM FACTOR: Balance Task Types =====
            int totalFreq = taskFrequency.Values.Sum();
            if (totalFreq > 0)
            {
                float thisFreq = taskFrequency[task];
                float avgFreq = totalFreq / taskFrequency.Count;

                if (thisFreq < avgFreq)
                {
                    score *= 1.15f; // Boost underutilized tasks
                }
            }

            // ===== MDP FACTOR 5: Material Availability =====
            if (!AreMaterialsAvailable(task))
            {
                score *= 0.3f; // Heavy penalty if materials missing
            }

            taskScores[task] = score;

            Debug.Log($"📊 Task: {task} | Score: {score:F1} | Time: {timeScore:F1} | Points: {pointScore:F1} | Recipe: {recipe.toyName}");
        }
    }

    TaskType GetTaskTypeFromRecipe(string recipeName)
    {
        switch (recipeName.ToLower())
        {
            case "bluetoy": return TaskType.BuildBlueToy;
            case "yellowtoy": return TaskType.BuildYellowToy;
            case "greentoy": return TaskType.BuildGreenToy;
            case "football": return TaskType.BuildFootball;
            case "hat": return TaskType.BuildHat;
            case "backpack": return TaskType.BuildBackpack;
            default: return TaskType.None;
        }
    }

    bool AreMaterialsAvailable(TaskType task)
    {
        switch (task)
        {
            case TaskType.BuildBlueToy:
                return FindItemByType(ItemData.ItemType.BluePLA) != null;
            case TaskType.BuildYellowToy:
                return FindItemByType(ItemData.ItemType.YellowPLA) != null;
            case TaskType.BuildGreenToy:
                return FindItemByType(ItemData.ItemType.GreenPLA) != null;
            case TaskType.BuildFootball:
                return FindLeatherByType(LeatherData.LeatherType.Brown) != null &&
                       FindLeatherByType(LeatherData.LeatherType.Purple) != null;
            case TaskType.BuildHat:
                return FindLeatherByType(LeatherData.LeatherType.Silver) != null &&
                       FindLeatherByType(LeatherData.LeatherType.Yellow) != null;
            case TaskType.BuildBackpack:
                return FindLeatherByType(LeatherData.LeatherType.Brown) != null &&
                       FindLeatherByType(LeatherData.LeatherType.Silver) != null;
            default:
                return false;
        }
    }

    TaskType ChooseBestTask()
    {
        if (taskScores.Count == 0) return TaskType.None;

        // Find task with highest score
        TaskType bestTask = TaskType.None;
        float bestScore = -1f;

        foreach (var kvp in taskScores)
        {
            if (kvp.Value > bestScore)
            {
                bestScore = kvp.Value;
                bestTask = kvp.Key;
            }
        }

        // Set target recipe name for verification
        List<Recipe> recipes = GetActiveRecipes();
        foreach (Recipe r in recipes)
        {
            if (GetTaskTypeFromRecipe(r.toyName) == bestTask)
            {
                targetRecipeName = r.toyName;
                break;
            }
        }

        return bestTask;
    }

    // ===== FSM STATE: EXECUTING TASK =====
    void UpdateExecutingTask()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("AI not on NavMesh, waiting...");
            return;
        }

        agent.isStopped = false;

        switch (currentTask)
        {
            case TaskType.BuildBlueToy:
            case TaskType.BuildYellowToy:
            case TaskType.BuildGreenToy:
                ExecutePrinterTask();
                break;
            case TaskType.BuildFootball:
            case TaskType.BuildHat:
            case TaskType.BuildBackpack:
                ExecuteCutterTask();
                break;
        }
    }

    void ExecutePrinterTask()
    {
        switch (currentPhase)
        {
            case ExecutionPhase.CollectingMaterial1:
                ItemData.ItemType neededPLA = GetPLATypeForTask(currentTask);
                GameObject pla = FindItemByType(neededPLA);

                if (pla == null)
                {
                    Debug.Log($"❌ PLA {neededPLA} not found, aborting task");
                    CompleteTask(false);
                    return;
                }

                NavigateAndInteract(pla, () => {
                    PickUpPLA(pla);
                    currentPhase = ExecutionPhase.LoadingMachine;
                });
                break;

            case ExecutionPhase.LoadingMachine:
                GameObject printer = FindClosestByTag("Printer");
                if (printer == null)
                {
                    Debug.Log("❌ No printer found");
                    CompleteTask(false);
                    return;
                }

                NavigateAndInteract(printer, () => {
                    PrinterLogic logic = printer.GetComponent<PrinterLogic>();
                    if (logic != null && logic.CanBake() && holdingPLA)
                    {
                        logic.ProcessItem(heldPLAType);
                        Destroy(heldItem);
                        heldItem = null;
                        holdingPLA = false;
                        currentPhase = ExecutionPhase.StartingMachine;
                        Debug.Log("✓ PLA loaded into printer");
                    }
                });
                break;

            case ExecutionPhase.StartingMachine:
                GameObject printerToStart = FindClosestByTag("Printer");
                if (printerToStart == null) return;

                NavigateAndInteract(printerToStart, () => {
                    PrinterLogic logic = printerToStart.GetComponent<PrinterLogic>();
                    if (logic != null && logic.CanBake())
                    {
                        // Press J to start (player would do this, AI simulates)
                        logic.GetType().GetMethod("StartCoroutine", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                            ?.Invoke(logic, new object[] { logic.GetType().GetMethod("BakeItem", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(logic, null) });
                        currentPhase = ExecutionPhase.WaitingForCompletion;
                        Debug.Log("✓ Printer started");
                    }
                });
                break;

            case ExecutionPhase.WaitingForCompletion:
                GameObject finishedPrinter = FindClosestByTag("FinishedPrinter");
                if (finishedPrinter != null)
                {
                    currentPhase = ExecutionPhase.CollectingFinished;
                }
                break;

            case ExecutionPhase.CollectingFinished:
                GameObject finished = FindClosestByTag("FinishedPrinter");
                if (finished == null)
                {
                    currentPhase = ExecutionPhase.WaitingForCompletion;
                    return;
                }

                NavigateAndInteract(finished, () => {
                    PickUpToyFromPrinter(finished);
                    currentPhase = ExecutionPhase.DeliveringToGiftbox;
                });
                break;

            case ExecutionPhase.DeliveringToGiftbox:
                GameObject giftbox = FindClosestByTag("Giftbox");
                if (giftbox == null || !holdingToy)
                {
                    CompleteTask(false);
                    return;
                }

                NavigateAndInteract(giftbox, () => {
                    DeliverToy();
                    CompleteTask(true);
                });
                break;
        }
    }

    void ExecuteCutterTask()
    {
        switch (currentPhase)
        {
            case ExecutionPhase.CollectingMaterial1:
                var leatherTypes = GetLeatherTypesForTask(currentTask);
                GameObject leather1 = FindLeatherByType(leatherTypes.Item1);

                if (leather1 == null)
                {
                    Debug.Log($"❌ Leather {leatherTypes.Item1} not found");
                    CompleteTask(false);
                    return;
                }

                NavigateAndInteract(leather1, () => {
                    PickUpLeather(leather1);
                    currentPhase = ExecutionPhase.LoadingMachine;
                });
                break;

            case ExecutionPhase.LoadingMachine:
                GameObject cutter1 = FindClosestByTag("LaserCutter");
                if (cutter1 == null)
                {
                    CompleteTask(false);
                    return;
                }

                NavigateAndInteract(cutter1, () => {
                    LaserCutterLogic logic = cutter1.GetComponent<LaserCutterLogic>();
                    if (logic != null && logic.CanLoadLeather() && holdingLeather)
                    {
                        logic.LoadLeather(heldLeatherType);
                        Destroy(heldItem);
                        heldItem = null;
                        holdingLeather = false;
                        currentPhase = ExecutionPhase.CollectingMaterial2;
                        Debug.Log("✓ First leather loaded");
                    }
                });
                break;

            case ExecutionPhase.CollectingMaterial2:
                var types = GetLeatherTypesForTask(currentTask);
                GameObject leather2 = FindLeatherByType(types.Item2);

                if (leather2 == null)
                {
                    Debug.Log($"❌ Second leather {types.Item2} not found");
                    CompleteTask(false);
                    return;
                }

                NavigateAndInteract(leather2, () => {
                    PickUpLeather(leather2);
                    currentPhase = ExecutionPhase.StartingMachine;
                });
                break;

            case ExecutionPhase.StartingMachine:
                GameObject cutter2 = FindClosestByTag("LaserCutter");
                if (cutter2 == null) return;

                NavigateAndInteract(cutter2, () => {
                    LaserCutterLogic logic = cutter2.GetComponent<LaserCutterLogic>();
                    if (logic != null && logic.CanLoadLeather() && holdingLeather)
                    {
                        logic.LoadLeather(heldLeatherType);
                        Destroy(heldItem);
                        heldItem = null;
                        holdingLeather = false;

                        // Start cutting if ready
                        if (logic.CanStartCutting())
                        {
                            logic.StartCutting();
                            currentPhase = ExecutionPhase.WaitingForCompletion;
                            Debug.Log("✓ Laser cutter started");
                        }
                    }
                });
                break;

            case ExecutionPhase.WaitingForCompletion:
                GameObject cutter3 = FindClosestByTag("LaserCutter");
                if (cutter3 != null)
                {
                    LaserCutterLogic logic = cutter3.GetComponent<LaserCutterLogic>();
                    if (logic != null && logic.IsFinished())
                    {
                        currentPhase = ExecutionPhase.CollectingFinished;
                    }
                }
                break;

            case ExecutionPhase.CollectingFinished:
                GameObject finishedCutter = FindClosestByTag("LaserCutter");
                if (finishedCutter == null) return;

                NavigateAndInteract(finishedCutter, () => {
                    PickUpCutterItem(finishedCutter);
                    currentPhase = ExecutionPhase.DeliveringToGiftbox;
                });
                break;

            case ExecutionPhase.DeliveringToGiftbox:
                GameObject giftbox = FindClosestByTag("Giftbox");
                if (giftbox == null || !holdingToy)
                {
                    CompleteTask(false);
                    return;
                }

                NavigateAndInteract(giftbox, () => {
                    DeliverToy();
                    CompleteTask(true);
                });
                break;
        }
    }

    void NavigateAndInteract(GameObject target, System.Action onReached)
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > interactRange)
        {
            agent.SetDestination(target.transform.position);
        }
        else
        {
            onReached?.Invoke();
        }
    }

    ItemData.ItemType GetPLATypeForTask(TaskType task)
    {
        switch (task)
        {
            case TaskType.BuildBlueToy: return ItemData.ItemType.BluePLA;
            case TaskType.BuildYellowToy: return ItemData.ItemType.YellowPLA;
            case TaskType.BuildGreenToy: return ItemData.ItemType.GreenPLA;
            default: return ItemData.ItemType.BluePLA;
        }
    }

    (LeatherData.LeatherType, LeatherData.LeatherType) GetLeatherTypesForTask(TaskType task)
    {
        switch (task)
        {
            case TaskType.BuildFootball:
                return (LeatherData.LeatherType.Brown, LeatherData.LeatherType.Purple);
            case TaskType.BuildHat:
                return (LeatherData.LeatherType.Silver, LeatherData.LeatherType.Yellow);
            case TaskType.BuildBackpack:
                return (LeatherData.LeatherType.Brown, LeatherData.LeatherType.Silver);
            default:
                return (LeatherData.LeatherType.Brown, LeatherData.LeatherType.Brown);
        }
    }

    GameObject FindItemByType(ItemData.ItemType type)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("PLA");
        foreach (var item in items)
        {
            ItemData data = item.GetComponent<ItemData>();
            if (data != null && data.itemType == type)
            {
                return item;
            }
        }
        return null;
    }

    GameObject FindLeatherByType(LeatherData.LeatherType type)
    {
        GameObject[] leathers = GameObject.FindGameObjectsWithTag("Leather");
        foreach (var leather in leathers)
        {
            LeatherData data = leather.GetComponent<LeatherData>();
            if (data != null && data.leatherType == type)
            {
                return leather;
            }
        }
        return null;
    }

    void CompleteTask(bool success)
    {
        if (success)
        {
            // N-gram: Track task completion
            AddToHistory(currentTask);
            taskFrequency[currentTask]++;
            Debug.Log($"✅ Task {currentTask} completed successfully!");
        }
        else
        {
            Debug.Log($"❌ Task {currentTask} failed");
        }

        currentTask = TaskType.None;
        targetRecipeName = "";
        currentPhase = ExecutionPhase.CollectingMaterial1;
        currentState = State.Idle;
        stateTimer = 1f;
    }

    // ===== ITEM INTERACTION =====
    void PickUpPLA(GameObject plaObject)
    {
        ItemData data = plaObject.GetComponent<ItemData>();
        if (data == null) return;

        heldPLAType = data.itemType;
        holdingPLA = true;
        holdingToy = false;

        GameObject prefab = data.itemPrefab != null ? data.itemPrefab : plaObject;
        heldItem = Instantiate(prefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.tag = "Untagged";

        DisablePhysics(heldItem);
        Destroy(plaObject);
    }

    void PickUpLeather(GameObject leatherObject)
    {
        LeatherData data = leatherObject.GetComponent<LeatherData>();
        if (data == null) return;

        heldLeatherType = data.leatherType;
        holdingLeather = true;
        holdingToy = false;

        GameObject prefab = data.leatherPrefab != null ? data.leatherPrefab : leatherObject;
        heldItem = Instantiate(prefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.tag = "Untagged";

        DisablePhysics(heldItem);
        Destroy(leatherObject);
    }

    void PickUpToyFromPrinter(GameObject printer)
    {
        PrinterLogic logic = printer.GetComponent<PrinterLogic>();
        if (logic == null) return;

        GameObject toyPrefab = logic.GetToyPrefab();
        if (toyPrefab == null) return;

        heldItem = Instantiate(toyPrefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.tag = "Untagged";
        holdingToy = true;
        holdingPLA = false;
        holdingLeather = false;

        heldItem.transform.localScale = Vector3.one * 2.5f;
        heldItem.transform.localRotation = Quaternion.identity;

        DisablePhysics(heldItem);
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

        string toyName = GetToyNameFromItem(heldItem);

        // Verify this matches our target recipe
        if (!toyName.Equals(targetRecipeName, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"⚠️ AI made wrong toy! Expected: {targetRecipeName}, Made: {toyName}");
            TrashItem();
            return;
        }

        if (recipeManager != null && recipeManager.HasMatchingRecipe(toyName))
        {
            int points = recipeManager.CompleteRecipe(toyName);

            Points pointsSystem = FindObjectOfType<Points>();
            if (pointsSystem != null) pointsSystem.AddPoints(points);

            if (quota != null) quota.QuotaProgressOne(1);

            Debug.Log($"🎁 AI delivered {toyName} for {points} points!");
        }

        Destroy(heldItem);
        heldItem = null;
        holdingToy = false;
    }

    void TrashItem()
    {
        Destroy(heldItem);
        heldItem = null;
        holdingPLA = false;
        holdingLeather = false;
        holdingToy = false;
    }

    string GetToyNameFromItem(GameObject item)
    {
        string name = item.name.Replace("(Clone)", "").Trim().ToLower();

        if (name.Contains("boat")) return "BlueToy";
        if (name.Contains("steamroller")) return "YellowToy";
        if (name.Contains("bricks")) return "GreenToy";
        if (name.Contains("football")) return "Football";
        if (name.Contains("hat")) return "Hat";
        if (name.Contains("backpack")) return "Backpack";

        return name;
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

    // ===== FSM STATE: FOLLOWING COMMAND =====
    void UpdateFollowingCommand()
    {
        if (!playerControlled)
        {
            currentState = State.Idle;
            return;
        }

        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 0.5f && targetObject != null)
        {
            TryInteractWithTarget();
        }
    }

    void TryInteractWithTarget()
    {
        float dist = Vector3.Distance(transform.position, targetObject.transform.position);
        if (dist > interactRange) return;

        if (targetObject.CompareTag("PLA") && !holdingPLA && !holdingLeather)
        {
            PickUpPLA(targetObject);
        }
        else if (targetObject.CompareTag("Leather") && !holdingLeather && !holdingPLA)
        {
            PickUpLeather(targetObject);
        }

        targetObject = null;
    }

    // ===== HELPER FUNCTIONS =====
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
        catch
        {
            return null;
        }
    }

    void AddToHistory(TaskType task)
    {
        taskHistory.Add(task);
        if (taskHistory.Count > maxHistory)
        {
            taskHistory.RemoveAt(0);
        }
    }
}
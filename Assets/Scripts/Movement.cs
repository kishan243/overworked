using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Movement Effects")]
    public GameObject particlePrefab;
    private ParticleSystem movementParticles;

    [Header("Interactions")]
    public Transform leftHand;
    public float interactRange = 3.5f;

    [Header("Sound Effects")]
    public List<AudioClip> footsteps;
    public float footstepDelay = 0.5f;
    public List<AudioClip> interactionSounds;

    private AudioSource audioSource;
    private float footstepTimer;
    private CharacterController controller;
    private GameObject heldItem;
    private ItemData.ItemType heldItemType;
    private LeatherData.LeatherType heldLeatherType;
    private bool heldItemIsToy = false;
    private bool heldItemIsLeather = false;
    private TextMeshProUGUI interactionText;
    private ItemPreviewUI itemPreviewUI; // Auto-found, no manual assignment needed

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.radius = 0.5f;
            controller.height = 2f;
            controller.center = new Vector3(0, 1f, 0);
        }

        GameObject textObj = GameObject.Find("InteractionText");

        if (textObj != null)
        {
            interactionText = textObj.GetComponent<TextMeshProUGUI>();
        }

        if (particlePrefab != null)
        {
            GameObject pObj = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            pObj.transform.SetParent(this.transform);
            pObj.transform.localPosition = new Vector3(0, 0.2f, 0);
            movementParticles = pObj.GetComponent<ParticleSystem>();
        }

        // Auto-find ItemPreviewUI in the scene
        itemPreviewUI = FindObjectOfType<ItemPreviewUI>();
        if (itemPreviewUI == null)
        {
            Debug.LogWarning("ItemPreviewUI not found in scene. Preview feature will be disabled.");
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        if (moveDir != Vector3.zero)
        {
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (movementParticles != null && !movementParticles.isEmitting)
            {
                movementParticles.Play();
            }

            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayRandomSound(footsteps);
                footstepTimer = footstepDelay;
            }
        }
        else
        {
            footstepTimer = 0f;

            if (movementParticles != null && movementParticles.isEmitting)
            {
                movementParticles.Stop();
            }
        }

        controller.Move(Vector3.down * 9.81f * Time.deltaTime);

        UpdateInteractionUI();
        UpdateItemPreview();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }
    }

    void UpdateItemPreview()
    {
        if (itemPreviewUI == null) return;

        // If holding an item, show it as "held"
        if (heldItem != null)
        {
            itemPreviewUI.ShowPreview(heldItem, true);
            return;
        }

        // Otherwise check what's nearby to pick up
        GameObject closestFinished = FindClosestByTag("FinishedPrinter");
        if (closestFinished != null)
        {
            PrinterLogic logic = closestFinished.GetComponent<PrinterLogic>();
            if (logic != null)
            {
                GameObject toyPrefab = logic.GetToyPrefab();
                if (toyPrefab != null)
                {
                    itemPreviewUI.ShowPreview(toyPrefab, false);
                    return;
                }
            }
        }

        GameObject closestPLA = FindClosestByTag("PLA");
        if (closestPLA != null)
        {
            ItemData data = closestPLA.GetComponent<ItemData>();
            if (data != null)
            {
                GameObject prefab = data.itemPrefab != null ? data.itemPrefab : closestPLA;
                itemPreviewUI.ShowPreview(prefab, false);
                return;
            }
        }

        GameObject closestLeather = FindClosestByTag("Leather");
        if (closestLeather != null)
        {
            LeatherData data = closestLeather.GetComponent<LeatherData>();
            if (data != null)
            {
                GameObject prefab = data.leatherPrefab != null ? data.leatherPrefab : closestLeather;
                itemPreviewUI.ShowPreview(prefab, false);
                return;
            }
        }

        GameObject closestCutter = FindClosestByTag("LaserCutter");
        if (closestCutter != null)
        {
            LaserCutterLogic logic = closestCutter.GetComponent<LaserCutterLogic>();
            if (logic != null && logic.IsFinished())
            {
                GameObject itemPrefab = logic.GetFinishedItem();
                if (itemPrefab != null)
                {
                    itemPreviewUI.ShowPreview(itemPrefab, false);
                    return;
                }
            }
        }

        // Nothing nearby, hide preview
        itemPreviewUI.HidePreview();
    }

    void UpdateInteractionUI()
    {
        if (interactionText == null) return;

        if (heldItem == null)
        {
            GameObject closestFinished = FindClosestByTag("FinishedPrinter");
            if (closestFinished != null) { interactionText.text = "Press SPACE to pick up toy"; return; }

            GameObject closestPLA = FindClosestByTag("PLA");
            if (closestPLA != null) { interactionText.text = "Press SPACE to pick up PLA"; return; }

            GameObject closestLeather = FindClosestByTag("Leather");
            if (closestLeather != null) { interactionText.text = "Press SPACE to pick up leather"; return; }

            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null)
            {
                PrinterLogic logic = closestPrinter.GetComponent<PrinterLogic>();
                if (logic != null && logic.CanBake()) { interactionText.text = "Press J to print"; return; }
            }

            GameObject closestCutter = FindClosestByTag("LaserCutter");
            if (closestCutter != null)
            {
                LaserCutterLogic logic = closestCutter.GetComponent<LaserCutterLogic>();
                if (logic != null)
                {
                    if (logic.IsCutting())
                    {
                        interactionText.text = "Cutting...";
                        return;
                    }
                    else if (logic.IsFinished())
                    {
                        interactionText.text = "Press SPACE to pick up item";
                        return;
                    }
                    else if (logic.CanStartCutting())
                    {
                        interactionText.text = "Press J to cut";
                        return;
                    }
                    else if (logic.CanLoadLeather())
                    {
                        interactionText.text = "Add leather to slots";
                        return;
                    }
                }
            }

            interactionText.text = "";
        }
        else
        {
            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null && !heldItemIsToy && !heldItemIsLeather)
            {
                interactionText.text = "Press SPACE to load printer";
                return;
            }

            GameObject closestCutter = FindClosestByTag("LaserCutter");
            if (closestCutter != null && heldItemIsLeather)
            {
                LaserCutterLogic logic = closestCutter.GetComponent<LaserCutterLogic>();
                if (logic != null && logic.CanLoadLeather())
                {
                    interactionText.text = "Press SPACE to load leather";
                    return;
                }
            }

            GameObject closestGiftbox = FindClosestByTag("Giftbox");
            if (closestGiftbox != null && heldItemIsToy)
            {
                FriedToyMarker friedMarker = heldItem.GetComponent<FriedToyMarker>();
                if (friedMarker != null && friedMarker.isFried)
                {
                    interactionText.text = "This toy is FRIED! Trash it";
                    return;
                }

                RecipeManager recipeManager = FindObjectOfType<RecipeManager>();
                string toyName = GetToyName();

                if (recipeManager != null && recipeManager.HasMatchingRecipe(toyName))
                {
                    interactionText.text = "Press SPACE to submit toy";
                    return;
                }
                else
                {
                    interactionText.text = "Wrong toy! Check recipes";
                    return;
                }
            }

            GameObject closestTrashcan = FindClosestByTag("Trashcan");
            if (closestTrashcan != null)
            {
                interactionText.text = "Press SPACE to trash item";
                return;
            }

            interactionText.text = "";
        }
    }

    GameObject FindClosestByTag(string tag)
    {
        try
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            GameObject closest = null;
            float closestDist = interactRange;

            foreach (GameObject obj in objects)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = obj;
                }
            }
            return closest;
        }
        catch (UnityException)
        {
            return null;
        }
    }

    void Interact()
    {
        bool isNearSomething = GetClosestInteractable() != null;

        if (isNearSomething)
        {
            PlayRandomSound(interactionSounds);
        }

        if (heldItem == null)
        {
            GameObject closestFinished = FindClosestByTag("FinishedPrinter");
            if (closestFinished != null) { PickUpToy(closestFinished); return; }

            GameObject closestPLA = FindClosestByTag("PLA");
            if (closestPLA != null) { PickUpPLA(closestPLA); return; }

            GameObject closestLeather = FindClosestByTag("Leather");
            if (closestLeather != null) { PickUpLeather(closestLeather); return; }

            GameObject closestCutter = FindClosestByTag("LaserCutter");
            if (closestCutter != null)
            {
                LaserCutterLogic logic = closestCutter.GetComponent<LaserCutterLogic>();
                if (logic != null && logic.IsFinished())
                {
                    PickUpCutterItem(closestCutter);
                    return;
                }
            }
        }
        else
        {
            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null && !heldItemIsToy && !heldItemIsLeather)
            {
                LoadPrinter(closestPrinter);
                return;
            }

            GameObject closestCutter = FindClosestByTag("LaserCutter");
            if (closestCutter != null && heldItemIsLeather)
            {
                LoadLaserCutter(closestCutter);
                return;
            }

            GameObject closestGiftbox = FindClosestByTag("Giftbox");
            if (closestGiftbox != null && heldItemIsToy)
            {
                FriedToyMarker friedMarker = heldItem.GetComponent<FriedToyMarker>();
                if (friedMarker == null || !friedMarker.isFried)
                {
                    RecipeManager recipeManager = FindObjectOfType<RecipeManager>();
                    string toyName = GetToyName();

                    if (recipeManager != null && recipeManager.HasMatchingRecipe(toyName))
                    {
                        SubmitToyToGiftbox();
                        return;
                    }
                }
            }

            GameObject closestTrashcan = FindClosestByTag("Trashcan");
            if (closestTrashcan != null)
            {
                TrashItem();
                return;
            }
        }
    }

    void PickUpToy(GameObject finishedPrinter)
    {
        PrinterLogic logic = finishedPrinter.GetComponent<PrinterLogic>();
        if (logic == null) return;

        GameObject toyPrefab = logic.GetToyPrefab();
        if (toyPrefab == null) return;

        if (logic.GetLoadedPLAType().HasValue)
        {
            heldItemType = logic.GetLoadedPLAType().Value;
        }

        heldItem = Instantiate(toyPrefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.tag = "Untagged";
        heldItemIsToy = true;
        heldItemIsLeather = false;

        if (logic.IsFried())
        {
            FriedToyMarker marker = heldItem.GetComponent<FriedToyMarker>();
            if (marker == null)
            {
                marker = heldItem.AddComponent<FriedToyMarker>();
            }
            marker.isFried = true;
        }

        if (heldItemType == ItemData.ItemType.GreenPLA)
        {
            heldItem.transform.localScale = Vector3.one * 0.3f;
        }
        else
        {
            heldItem.transform.localScale = Vector3.one * 2.5f;
        }

        Collider col = heldItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody heldRb = heldItem.GetComponent<Rigidbody>();
        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.detectCollisions = false;
        }

        heldItem.transform.localRotation = Quaternion.identity;
        logic.ClearAfterPickup();
    }

    void PickUpPLA(GameObject plaObject)
    {
        ItemData data = plaObject.GetComponent<ItemData>();
        if (data == null) return;

        heldItemType = data.itemType;
        heldItemIsToy = false;
        heldItemIsLeather = false;

        heldItem = Instantiate(data.itemPrefab != null ? data.itemPrefab : plaObject, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.tag = "Untagged";

        if (heldItem.TryGetComponent<Collider>(out Collider c)) c.enabled = false;
        if (heldItem.TryGetComponent<Rigidbody>(out Rigidbody r))
        {
            r.isKinematic = true;
            r.detectCollisions = false;
        }

        heldItem.transform.localRotation = Quaternion.identity;
    }

    void PickUpLeather(GameObject leatherObject)
    {
        LeatherData data = leatherObject.GetComponent<LeatherData>();
        if (data == null) return;

        heldLeatherType = data.leatherType;
        heldItemIsToy = false;
        heldItemIsLeather = true;

        heldItem = Instantiate(data.leatherPrefab != null ? data.leatherPrefab : leatherObject, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.tag = "Untagged";

        if (heldItem.TryGetComponent<Collider>(out Collider c)) c.enabled = false;
        if (heldItem.TryGetComponent<Rigidbody>(out Rigidbody r))
        {
            r.isKinematic = true;
            r.detectCollisions = false;
        }

        heldItem.transform.localRotation = Quaternion.identity;
    }

    void PickUpCutterItem(GameObject cutterObject)
    {
        LaserCutterLogic logic = cutterObject.GetComponent<LaserCutterLogic>();
        if (logic == null) return;

        GameObject itemPrefab = logic.GetFinishedItem();
        if (itemPrefab == null) return;

        heldItem = Instantiate(itemPrefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.tag = "Untagged";
        heldItemIsToy = true;
        heldItemIsLeather = false;

        string itemName = itemPrefab.name.ToLower();
        if (itemName.Contains("hat") || itemName.Contains("backpack"))
        {
            heldItem.transform.localScale = Vector3.one * 3.5f;
        }
        else
        {
            heldItem.transform.localScale = Vector3.one * 2.0f;
        }

        Collider col = heldItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody heldRb = heldItem.GetComponent<Rigidbody>();
        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.detectCollisions = false;
        }

        heldItem.transform.localRotation = Quaternion.identity;
        logic.ClearAfterPickup();
    }

    void LoadPrinter(GameObject printerObj)
    {
        PrinterLogic printer = printerObj.GetComponent<PrinterLogic>();
        if (printer != null)
        {
            printer.ProcessItem(heldItemType);
            Destroy(heldItem);
            heldItem = null;
            heldItemIsToy = false;
            heldItemIsLeather = false;
        }
    }

    void LoadLaserCutter(GameObject cutterObj)
    {
        LaserCutterLogic cutter = cutterObj.GetComponent<LaserCutterLogic>();
        if (cutter != null && cutter.CanLoadLeather())
        {
            cutter.LoadLeather(heldLeatherType);
            Destroy(heldItem);
            heldItem = null;
            heldItemIsLeather = false;
        }
    }

    void SubmitToyToGiftbox()
    {
        RecipeManager recipeManager = FindObjectOfType<RecipeManager>();
        string toyName = GetToyName();

        // Get points from recipe completion
        int pointsEarned = 0;
        if (recipeManager != null)
        {
            pointsEarned = recipeManager.CompleteRecipe(toyName);
        }

        // Add points to Points system
        Points pointsSystem = FindObjectOfType<Points>();
        if (pointsSystem != null)
        {
            pointsSystem.AddPoints(pointsEarned);
        }

        // Update quota
        Quota quota = FindObjectOfType<Quota>();
        if (quota != null)
        {
            quota.QuotaProgressOne(1);
        }

        // Destroy the toy/item
        Destroy(heldItem);
        heldItem = null;
        heldItemIsToy = false;
        heldItemIsLeather = false;
    }

    string GetToyName()
    {
        ItemData itemData = heldItem.GetComponent<ItemData>();

        if (itemData != null)
        {
            switch (heldItemType)
            {
                case ItemData.ItemType.BluePLA: return "BlueToy";
                case ItemData.ItemType.YellowPLA: return "YellowToy";
                case ItemData.ItemType.GreenPLA: return "GreenToy";
                default: return "Unknown";
            }
        }
        else
        {
            return GetItemName(heldItem);
        }
    }

    string GetItemName(GameObject item)
    {
        string name = item.name.Replace("(Clone)", "").Trim();

        if (name.ToLower().Contains("football")) return "Football";
        if (name.ToLower().Contains("backpack")) return "Backpack";
        if (name.ToLower().Contains("hat")) return "Hat";

        return name;
    }

    void TrashItem()
    {
        Destroy(heldItem);
        heldItem = null;
        heldItemIsToy = false;
        heldItemIsLeather = false;
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayRandomSound(List<AudioClip> soundEffectsList)
    {
        if (soundEffectsList.Count > 0)
        {
            int randomIndex = Random.Range(0, soundEffectsList.Count);
            PlaySound(soundEffectsList[randomIndex]);
        }
    }

    GameObject GetClosestInteractable()
    {
        string[] tags = { "FinishedPrinter", "PLA", "Leather", "LaserCutter", "Printer", "Giftbox", "Trashcan" };

        foreach (string tag in tags)
        {
            GameObject obj = FindClosestByTag(tag);
            if (obj != null) return obj;
        }
        return null;
    }
}

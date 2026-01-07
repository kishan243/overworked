using UnityEngine;
using TMPro;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10f;

    [Header("Interactions")]
    public Transform leftHand;
    public float interactRange = 3.5f;
    private GameObject heldItem;
    private ItemData.ItemType heldItemType;
    private bool heldItemIsToy = false;

    private Rigidbody rb;
    private Vector3 moveDir;
    private TextMeshProUGUI interactionText;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        GameObject textObj = GameObject.Find("InteractionText");
        if (textObj != null)
        {
            interactionText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(x, 0, z).normalized;

        UpdateInteractionUI();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }
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

            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null)
            {
                PrinterLogic logic = closestPrinter.GetComponent<PrinterLogic>();
                if (logic != null && logic.CanBake()) { interactionText.text = "Press J to bake"; return; }
            }
            interactionText.text = "";
        }
        else
        {
            // If holding PLA, can load printer
            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null && !heldItemIsToy)
            {
                interactionText.text = "Press SPACE to load printer";
                return;
            }

            // If holding toy, can submit to giftbox (only if NOT fried)
            GameObject closestGiftbox = FindClosestByTag("Giftbox");
            if (closestGiftbox != null && heldItemIsToy)
            {
                // Check if the toy is fried
                FriedToyMarker friedMarker = heldItem.GetComponent<FriedToyMarker>();
                if (friedMarker == null || !friedMarker.isFried)
                {
                    interactionText.text = "Press SPACE to submit toy";
                    return;
                }
            }

            // Can always trash
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
        if (heldItem == null)
        {
            GameObject closestFinished = FindClosestByTag("FinishedPrinter");
            if (closestFinished != null) { PickUpToy(closestFinished); return; }

            GameObject closestPLA = FindClosestByTag("PLA");
            if (closestPLA != null) { PickUpPLA(closestPLA); return; }
        }
        else
        {
            // If holding PLA, try to load printer
            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null && !heldItemIsToy)
            {
                LoadPrinter(closestPrinter);
                return;
            }

            // If holding toy, try to submit to giftbox (only if NOT fried)
            GameObject closestGiftbox = FindClosestByTag("Giftbox");
            if (closestGiftbox != null && heldItemIsToy)
            {
                FriedToyMarker friedMarker = heldItem.GetComponent<FriedToyMarker>();
                if (friedMarker == null || !friedMarker.isFried)
                {
                    SubmitToyToGiftbox();
                    return;
                }
            }

            // Try to trash item
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

        heldItem = Instantiate(toyPrefab, leftHand.position, leftHand.rotation, leftHand);
        heldItem.tag = "Untagged";
        heldItemIsToy = true;

        // Check if fried
        if (logic.IsFried())
        {
            FriedToyMarker marker = heldItem.GetComponent<FriedToyMarker>();
            if (marker == null)
            {
                marker = heldItem.AddComponent<FriedToyMarker>();
            }
            marker.isFried = true;
        }

        // Get the color from the printer's loaded type and set scale
        if (logic.GetLoadedPLAType().HasValue)
        {
            heldItemType = logic.GetLoadedPLAType().Value;
        }

        // THE FIX: Scale based on color - works for BOTH normal AND fried toys
        if (heldItemType == ItemData.ItemType.GreenPLA)
        {
            heldItem.transform.localScale = Vector3.one * 0.4f;  // Green is smaller
        }
        else
        {
            heldItem.transform.localScale = Vector3.one * 2.0f;  // Blue/Yellow are bigger
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

    void LoadPrinter(GameObject printerObj)
    {
        PrinterLogic printer = printerObj.GetComponent<PrinterLogic>();
        if (printer != null)
        {
            printer.ProcessItem(heldItemType);
            Destroy(heldItem);
            heldItem = null;
            heldItemIsToy = false;
        }
    }

    void SubmitToyToGiftbox()
    {
        // Find Quota script and update it
        Quota quota = FindObjectOfType<Quota>();
        if (quota != null)
        {
            quota.QuotaProgressOne(1);
        }

        // Destroy the toy
        Destroy(heldItem);
        heldItem = null;
        heldItemIsToy = false;
    }

    void TrashItem()
    {
        // Simply destroy whatever is being held
        Destroy(heldItem);
        heldItem = null;
        heldItemIsToy = false;
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);

        if (moveDir != Vector3.zero) transform.forward = moveDir;
    }
}
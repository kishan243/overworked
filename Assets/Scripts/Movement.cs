using UnityEngine;
using TMPro;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;

    [Header("Interactions")]
    public Transform leftHand;
    public float interactRange = 3.5f;
    private GameObject heldItem;
    private ItemData.ItemType heldItemType;

    private Rigidbody rb;
    private Vector3 moveDir;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashDir;
    private TextMeshProUGUI interactionText;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // FIX FOR RANDOM SPINNING: Freeze all physics rotation.
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

        if (Input.GetKeyDown(KeyCode.L) && !isDashing && moveDir.magnitude > 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDir = moveDir;
        }

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
            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null) { interactionText.text = "Press SPACE to load printer"; }
            else { interactionText.text = "Press SPACE to drop item"; }
        }
    }

    GameObject FindClosestByTag(string tag)
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
            GameObject closestPrinter = FindClosestByTag("Printer");
            if (closestPrinter != null) { LoadPrinter(closestPrinter); }
            else { DropItem(); }
        }
    }

    void PickUpToy(GameObject finishedPrinter)
    {
        PrinterLogic logic = finishedPrinter.GetComponent<PrinterLogic>();
        if (logic == null) return;

        GameObject toyPrefab = logic.GetToyPrefab();
        if (toyPrefab == null) return;

        heldItem = Instantiate(toyPrefab, leftHand.position, leftHand.rotation, leftHand);

        // --- SCALING FIX ---
        // Instead of Vector3.one, we set specific scales based on the item
        // You can adjust these numbers until they look perfect
        if (toyPrefab.name.Contains("Green"))
        {
            heldItem.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Make green smaller
        }
        else
        {
            heldItem.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // Make others larger
        }

        heldItem.tag = "Untagged";

        // ... (rest of your existing logic for Colliders/Rigidbodies)
        Collider col = heldItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody heldRb = heldItem.GetComponent<Rigidbody>();
        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.detectCollisions = false;
        }

        heldItem.transform.localRotation = Quaternion.identity;
        logic.ResetToEmpty();
    }

    void PickUpPLA(GameObject plaObject)
    {
        ItemData data = plaObject.GetComponent<ItemData>();
        if (data == null) return;
        heldItemType = data.itemType;

        heldItem = Instantiate(data.itemPrefab != null ? data.itemPrefab : plaObject, leftHand.position, leftHand.rotation, leftHand);
        heldItem.transform.localScale = Vector3.one * 0.4f;
        heldItem.tag = "Untagged";

        if (heldItem.TryGetComponent<Collider>(out Collider c)) c.enabled = false;
        if (heldItem.TryGetComponent<Rigidbody>(out Rigidbody r)) { r.isKinematic = true; r.detectCollisions = false; }

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
        }
    }

    void DropItem()
    {
        if (heldItem == null) return;

        heldItem.transform.parent = null;
        heldItem.transform.position = transform.position + transform.forward * 1.5f;
        heldItem.transform.localScale = Vector3.one * 0.3f;
        heldItem.tag = "PLA";

        if (heldItem.TryGetComponent<Collider>(out Collider c)) c.enabled = true;
        if (heldItem.TryGetComponent<Rigidbody>(out Rigidbody r))
        {
            r.isKinematic = false;
            r.detectCollisions = true;
            r.useGravity = true;
            r.velocity = Vector3.zero;
        }

        // Re-enable item data
        ItemData heldData = heldItem.GetComponent<ItemData>();
        if (heldData == null) heldData = heldItem.AddComponent<ItemData>();
        heldData.itemType = heldItemType;

        heldItem = null;
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = new Vector3(dashDir.x * dashSpeed, 0, dashDir.z * dashSpeed);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0) isDashing = false;
        }
        else
        {
            rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);
        }

        if (moveDir != Vector3.zero) transform.forward = moveDir;
    }
}
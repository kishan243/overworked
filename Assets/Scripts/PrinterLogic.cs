using UnityEngine;
using System.Collections;

public class PrinterLogic : MonoBehaviour
{
    [Header("Blue PLA")]
    public GameObject bluePrinterPrefab;
    public GameObject blueBakedPrefab;
    public GameObject blueFinishedItemPrefab;

    [Header("Yellow PLA")]
    public GameObject yellowPrinterPrefab;
    public GameObject yellowBakedPrefab;
    public GameObject yellowFinishedItemPrefab;

    [Header("Green PLA")]
    public GameObject greenPrinterPrefab;
    public GameObject greenBakedPrefab;
    public GameObject greenFinishedItemPrefab;

    [Header("Settings")]
    public float bakeTime = 14f;
    public GameObject basePrinterPrefab; // Drag "Empty Printer" prefab here!

    private bool isBaking = false;
    private bool isFinished = false;
    private ItemData.ItemType? loadedPLAType = null;
    private GameObject toyPrefab;

    void Start()
    {
        if (isBaking)
        {
            StartCoroutine(FinishBaking());
        }
    }

    // Update this method in PrinterLogic.cs
    public void InitializeFromPrevious(PrinterLogic oldLogic, bool bakingState)
    {
        // Ensure the base reference is carried over to the new instance
        this.basePrinterPrefab = oldLogic.basePrinterPrefab;

        this.loadedPLAType = oldLogic.loadedPLAType;
        this.toyPrefab = oldLogic.toyPrefab;
        this.isBaking = bakingState;

        // If it was already baking, we need to resume the timer
        if (this.isBaking)
        {
            StartCoroutine(FinishBaking());
        }
    }

    void Update()
    {
        if (loadedPLAType != null && !isBaking && !isFinished && Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(BakeItem());
        }
    }

    public void ProcessItem(ItemData.ItemType itemType)
    {
        if (isBaking || isFinished) return;

        GameObject targetPrefab = null;

        switch (itemType)
        {
            case ItemData.ItemType.BluePLA:
                targetPrefab = bluePrinterPrefab;
                loadedPLAType = ItemData.ItemType.BluePLA;
                break;
            case ItemData.ItemType.YellowPLA:
                targetPrefab = yellowPrinterPrefab;
                loadedPLAType = ItemData.ItemType.YellowPLA;
                break;
            case ItemData.ItemType.GreenPLA:
                targetPrefab = greenPrinterPrefab;
                loadedPLAType = ItemData.ItemType.GreenPLA;
                break;
        }

        if (targetPrefab != null)
        {
            StartCoroutine(TransformPrinter(targetPrefab));
        }
    }

    IEnumerator TransformPrinter(GameObject newPrefab)
    {
        yield return new WaitForSeconds(0.5f);
        SwapPrefab(newPrefab, false);
    }

    IEnumerator BakeItem()
    {
        isBaking = true;
        GameObject bakedPrefab = null;

        switch (loadedPLAType.Value)
        {
            case ItemData.ItemType.BluePLA:
                bakedPrefab = blueBakedPrefab;
                toyPrefab = blueFinishedItemPrefab;
                break;
            case ItemData.ItemType.YellowPLA:
                bakedPrefab = yellowBakedPrefab;
                toyPrefab = yellowFinishedItemPrefab;
                break;
            case ItemData.ItemType.GreenPLA:
                bakedPrefab = greenBakedPrefab;
                toyPrefab = greenFinishedItemPrefab;
                break;
        }

        if (bakedPrefab != null)
        {
            SwapPrefab(bakedPrefab, true);
        }

        // --- THE FIX: This line was missing, causing error CS0161 ---
        yield return null;
    }

    IEnumerator FinishBaking()
    {
        yield return new WaitForSeconds(bakeTime);
        isBaking = false;
        isFinished = true;
        gameObject.tag = "FinishedPrinter";
    }

    public void ResetToEmpty()
    {
        if (basePrinterPrefab != null)
        {
            GameObject newPrinter = Instantiate(basePrinterPrefab, transform.position, transform.rotation, transform.parent);
            newPrinter.tag = "Printer";
            newPrinter.layer = gameObject.layer;
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Base Printer Prefab is missing! Drag your Empty Printer Prefab into the script slot.");
        }
    }

    void SwapPrefab(GameObject newPrefab, bool bakingState)
    {
        GameObject newPrinter = Instantiate(newPrefab, transform.position, transform.rotation, transform.parent);
        newPrinter.tag = "Printer";
        newPrinter.layer = gameObject.layer;

        PrinterLogic newLogic = newPrinter.GetComponent<PrinterLogic>();
        if (newLogic != null)
        {
            newLogic.InitializeFromPrevious(this, bakingState);
        }

        Destroy(gameObject);
    }

    public bool CanBake()
    {
        return loadedPLAType != null && !isBaking && !isFinished;
    }

    public GameObject GetToyPrefab()
    {
        return toyPrefab;
    }
}
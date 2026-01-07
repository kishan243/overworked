using UnityEngine;
using System.Collections;

public class PrinterLogic : MonoBehaviour
{
    [Header("Blue PLA")]
    public GameObject bluePrinterPrefab;
    public GameObject blueBakedPrefab;
    public GameObject blueFinishedItemPrefab;
    public GameObject blueFriedItemPrefab;

    [Header("Yellow PLA")]
    public GameObject yellowPrinterPrefab;
    public GameObject yellowBakedPrefab;
    public GameObject yellowFinishedItemPrefab;
    public GameObject yellowFriedItemPrefab;

    [Header("Green PLA")]
    public GameObject greenPrinterPrefab;
    public GameObject greenBakedPrefab;
    public GameObject greenFinishedItemPrefab;
    public GameObject greenFriedItemPrefab;

    [Header("Settings")]
    public float bakeTime = 8f;
    public float friedTime = 5f;
    public GameObject basePrinterPrefab;

    private bool isBaking = false;
    private bool isFinished = false;
    private bool isFried = false;
    private ItemData.ItemType? loadedPLAType = null;
    private GameObject toyPrefab;
    private GameObject friedToyPrefab;
    private GameObject warningCube;

    void Start()
    {
        if (isBaking)
        {
            StartCoroutine(FinishBaking());
        }
    }

    public void InitializeFromPrevious(PrinterLogic oldLogic, bool bakingState)
    {
        this.basePrinterPrefab = oldLogic.basePrinterPrefab;
        this.bluePrinterPrefab = oldLogic.bluePrinterPrefab;
        this.blueBakedPrefab = oldLogic.blueBakedPrefab;
        this.blueFinishedItemPrefab = oldLogic.blueFinishedItemPrefab;
        this.blueFriedItemPrefab = oldLogic.blueFriedItemPrefab;
        this.yellowPrinterPrefab = oldLogic.yellowPrinterPrefab;
        this.yellowBakedPrefab = oldLogic.yellowBakedPrefab;
        this.yellowFinishedItemPrefab = oldLogic.yellowFinishedItemPrefab;
        this.yellowFriedItemPrefab = oldLogic.yellowFriedItemPrefab;
        this.greenPrinterPrefab = oldLogic.greenPrinterPrefab;
        this.greenBakedPrefab = oldLogic.greenBakedPrefab;
        this.greenFinishedItemPrefab = oldLogic.greenFinishedItemPrefab;
        this.greenFriedItemPrefab = oldLogic.greenFriedItemPrefab;
        this.bakeTime = oldLogic.bakeTime;
        this.friedTime = oldLogic.friedTime;

        this.loadedPLAType = oldLogic.loadedPLAType;
        this.toyPrefab = oldLogic.toyPrefab;
        this.friedToyPrefab = oldLogic.friedToyPrefab;
        this.isBaking = bakingState;

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

        // Make the cube bob up and down if it exists
        if (warningCube != null)
        {
            float bobAmount = Mathf.Sin(Time.time * 3f) * 0.15f;
            warningCube.transform.localPosition = new Vector3(0, 2f + bobAmount, 0);
        }
    }

    public void ProcessItem(ItemData.ItemType itemType)
    {
        if (isBaking) return;

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
                friedToyPrefab = blueFriedItemPrefab;
                break;
            case ItemData.ItemType.YellowPLA:
                bakedPrefab = yellowBakedPrefab;
                toyPrefab = yellowFinishedItemPrefab;
                friedToyPrefab = yellowFriedItemPrefab;
                break;
            case ItemData.ItemType.GreenPLA:
                bakedPrefab = greenBakedPrefab;
                toyPrefab = greenFinishedItemPrefab;
                friedToyPrefab = greenFriedItemPrefab;
                break;
        }

        if (bakedPrefab != null)
        {
            SwapPrefab(bakedPrefab, true);
        }

        yield return null;
    }

    IEnumerator FinishBaking()
    {
        yield return new WaitForSeconds(bakeTime);
        isBaking = false;
        isFinished = true;
        gameObject.tag = "FinishedPrinter";

        // Only spawn warning cube if toy is still here
        if (isFinished)
        {
            warningCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            warningCube.transform.SetParent(transform);
            warningCube.transform.localPosition = new Vector3(0, 5f, 0);
            warningCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Destroy(warningCube.GetComponent<Collider>());

            StartCoroutine(FryTimerWithColorChange());
        }
    }

    IEnumerator FryTimerWithColorChange()
    {
        float elapsed = 0f;

        // Transition from green -> yellow -> orange -> red -> NEON RED
        while (elapsed < friedTime && warningCube != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / friedTime; // 0 to 1

            // Check if cube still exists before accessing renderer
            if (warningCube == null) yield break;

            Renderer cubeRenderer = warningCube.GetComponent<Renderer>();
            if (cubeRenderer == null) yield break;

            Color currentColor;
            if (t < 0.33f)
            {
                // Green to Yellow
                currentColor = Color.Lerp(Color.green, Color.yellow, t / 0.33f);
            }
            else if (t < 0.66f)
            {
                // Yellow to Orange
                currentColor = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (t - 0.33f) / 0.33f);
            }
            else
            {
                // Orange to Red
                currentColor = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (t - 0.66f) / 0.34f);
            }

            cubeRenderer.material.color = currentColor;
            yield return null;
        }

        // Check if cube still exists before making it neon red
        if (warningCube == null) yield break;

        Renderer finalRenderer = warningCube.GetComponent<Renderer>();
        if (finalRenderer == null) yield break;

        // NOW IT'S FRIED! Make it NEON RED
        isFried = true;
        toyPrefab = friedToyPrefab;

        // Bright neon red
        Color neonRed = new Color(1f, 0f, 0f) * 2f;
        finalRenderer.material.color = neonRed;

        Debug.Log("TOY IS FRIED! Neon red warning!");
    }

    public void ClearAfterPickup()
    {
        // Stop all coroutines to prevent accessing destroyed objects
        StopAllCoroutines();

        isFinished = false;
        isBaking = false;
        isFried = false;
        loadedPLAType = null;
        toyPrefab = null;
        friedToyPrefab = null;
        gameObject.tag = "Printer";

        // Remove the warning cube
        if (warningCube != null)
        {
            Destroy(warningCube);
            warningCube = null;
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

    public ItemData.ItemType? GetLoadedPLAType()
    {
        return loadedPLAType;
    }

    public bool IsFried()
    {
        return isFried;
    }
}
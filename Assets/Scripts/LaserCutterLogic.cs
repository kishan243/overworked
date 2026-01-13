using UnityEngine;
using System.Collections;

public class LaserCutterLogic : MonoBehaviour
{
    [System.Serializable]
    public class Recipe
    {
        public LeatherData.LeatherType leather1;
        public LeatherData.LeatherType leather2;
        public GameObject resultPrefab;
        public string itemName;
    }

    [Header("Recipes")]
    public Recipe[] recipes;

    [Header("Settings")]
    public float cuttingTime = 10f;
    public float uiHeight = 4f;
    public float interactRange = 3.5f;

    private LeatherData.LeatherType? slot1 = null;
    private LeatherData.LeatherType? slot2 = null;
    private bool isCutting = false;
    private bool isFinished = false;
    private GameObject finishedItemPrefab;
    private GameObject slot1Cube;
    private GameObject slot2Cube;
    private GameObject progressCube;

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        slot1Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slot1Cube.transform.SetParent(transform);
        slot1Cube.transform.localPosition = new Vector3(-0.6f, uiHeight, 0);
        slot1Cube.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        Destroy(slot1Cube.GetComponent<Collider>());
        slot1Cube.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        slot2Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slot2Cube.transform.SetParent(transform);
        slot2Cube.transform.localPosition = new Vector3(0.6f, uiHeight, 0);
        slot2Cube.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        Destroy(slot2Cube.GetComponent<Collider>());
        slot2Cube.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && !isCutting && !isFinished)
        {
            if (IsPlayerNearby()) RemoveLastLeather();
        }

        if (Input.GetKeyDown(KeyCode.J) && CanStartCutting())
        {
            if (IsPlayerNearby()) StartCutting();
        }

        if (progressCube != null)
        {
            float bobAmount = Mathf.Sin(Time.time * 3f) * 0.15f;
            progressCube.transform.localPosition = new Vector3(0, uiHeight + 0.75f + bobAmount, 0);
        }
    }

    bool IsPlayerNearby()
    {
        Movement player = FindObjectOfType<Movement>();
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            return distance <= interactRange;
        }
        return false;
    }

    void UpdateUI()
    {
        if (slot1.HasValue && slot1Cube != null)
        {
            slot1Cube.GetComponent<Renderer>().material.color = GetLeatherColor(slot1.Value);
        }
        else if (slot1Cube != null)
        {
            slot1Cube.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        if (slot2.HasValue && slot2Cube != null)
        {
            slot2Cube.GetComponent<Renderer>().material.color = GetLeatherColor(slot2.Value);
        }
        else if (slot2Cube != null)
        {
            slot2Cube.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    Color GetLeatherColor(LeatherData.LeatherType type)
    {
        switch (type)
        {
            case LeatherData.LeatherType.Brown: return new Color(0.6f, 0.3f, 0.1f);
            case LeatherData.LeatherType.Purple: return new Color(0.5f, 0.2f, 0.7f);
            case LeatherData.LeatherType.Silver: return new Color(0.7f, 0.7f, 0.7f);
            case LeatherData.LeatherType.Yellow: return new Color(0.9f, 0.9f, 0.2f);
            default: return Color.white;
        }
    }

    public bool CanLoadLeather()
    {
        return !isCutting && !isFinished && (!slot1.HasValue || !slot2.HasValue);
    }

    public void LoadLeather(LeatherData.LeatherType type)
    {
        if (!slot1.HasValue) slot1 = type;
        else if (!slot2.HasValue) slot2 = type;
        UpdateUI();
    }

    void RemoveLastLeather()
    {
        if (slot2.HasValue) slot2 = null;
        else if (slot1.HasValue) slot1 = null;
        UpdateUI();
    }

    public bool CanStartCutting()
    {
        if (!slot1.HasValue || !slot2.HasValue || isCutting || isFinished) return false;
        return GetRecipeResult(slot1.Value, slot2.Value) != null;
    }

    public void StartCutting()
    {
        if (CanStartCutting()) StartCoroutine(CutItem());
    }

    IEnumerator CutItem()
    {
        isCutting = true;

        progressCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        progressCube.transform.SetParent(transform);
        progressCube.transform.localPosition = new Vector3(0, uiHeight + 0.75f, 0);
        progressCube.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        Destroy(progressCube.GetComponent<Collider>());

        float elapsed = 0f;
        Renderer cubeRenderer = progressCube.GetComponent<Renderer>();

        while (elapsed < cuttingTime && progressCube != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cuttingTime;

            if (cubeRenderer != null)
            {
                Color currentColor = Color.Lerp(Color.yellow, Color.green, t);
                cubeRenderer.material.color = currentColor;
            }

            yield return null;
        }

        if (cubeRenderer != null) cubeRenderer.material.color = Color.green;

        isCutting = false;
        isFinished = true;
        finishedItemPrefab = GetRecipeResult(slot1.Value, slot2.Value);
    }

    GameObject GetRecipeResult(LeatherData.LeatherType type1, LeatherData.LeatherType type2)
    {
        foreach (Recipe recipe in recipes)
        {
            if ((recipe.leather1 == type1 && recipe.leather2 == type2) ||
                (recipe.leather1 == type2 && recipe.leather2 == type1))
            {
                return recipe.resultPrefab;
            }
        }
        return null;
    }

    public GameObject GetFinishedItem()
    {
        return finishedItemPrefab;
    }

    public void ClearAfterPickup()
    {
        StopAllCoroutines();
        slot1 = null;
        slot2 = null;
        isCutting = false;
        isFinished = false;
        finishedItemPrefab = null;

        if (progressCube != null)
        {
            Destroy(progressCube);
            progressCube = null;
        }

        UpdateUI();
    }

    public bool IsFinished()
    {
        return isFinished;
    }

    public bool IsCutting()
    {
        return isCutting;
    }
}
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual feedback component for workstations.
/// Displays progress bar and station label.
/// </summary>
public class WorkStationVisuals : MonoBehaviour
{
    public WorkStation workStation;
    public GameObject progressBarParent;
    public Image progressBarFill;
    public Text stationLabel;
    public Color availableColor = Color.green;
    public Color occupiedColor = Color.yellow;
    public Color completedColor = Color.cyan;

    private Renderer stationRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        if (workStation == null)
            workStation = GetComponent<WorkStation>();
        
        stationRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (stationLabel != null)
        {
            stationLabel.text = workStation.stationType;
        }

        if (progressBarParent != null)
        {
            progressBarParent.SetActive(false);
        }
    }

    void Update()
    {
        UpdateProgressBar();
        UpdateStationColor();
    }

    void UpdateProgressBar()
    {
        if (progressBarFill != null && progressBarParent != null)
        {
            bool isWorking = !workStation.IsAvailable();
            progressBarParent.SetActive(isWorking);

            if (isWorking)
            {
                float progress = workStation.GetProgress();
                progressBarFill.fillAmount = progress;
            }
        }
    }

    void UpdateStationColor()
    {
        if (stationRenderer == null)
            return;

        Color targetColor = availableColor;
        
        if (!workStation.IsAvailable())
        {
            float progress = workStation.GetProgress();
            targetColor = progress >= 1.0f ? completedColor : occupiedColor;
        }

        stationRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", targetColor);
        stationRenderer.SetPropertyBlock(propertyBlock);
    }
}

using UnityEngine;

public class CharacterSwapper : MonoBehaviour
{
    public PlayerController playerController;
    public AIController aiController;
    public Camera mainCamera;
    public float cameraOffset = 10f;
    public float cameraHeight = 8f;

    private bool isControllingPlayer = true;

    private static CharacterSwapper instance;
    public static CharacterSwapper Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        if (playerController == null)
            playerController = GameObject.FindObjectOfType<PlayerController>();
        
        if (aiController == null)
            aiController = GameObject.FindObjectOfType<AIController>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        InitializeCharacters();
    }

    void InitializeCharacters()
    {
        if (playerController != null)
        {
            playerController.SetPlayerControlled(true);
        }

        if (aiController != null)
        {
            aiController.SetAIControlled(true);
        }

        UpdateCamera();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwapCharacters();
        }

        UpdateCamera();
    }

    void SwapCharacters()
    {
        isControllingPlayer = !isControllingPlayer;

        if (playerController != null && aiController != null)
        {
            // Swap control
            playerController.SetPlayerControlled(isControllingPlayer);
            aiController.SetAIControlled(isControllingPlayer);

            // Swap positions
            Vector3 tempPosition = playerController.transform.position;
            playerController.transform.position = aiController.transform.position;
            aiController.transform.position = tempPosition;

            Debug.Log("Swapped characters! Now controlling: " + (isControllingPlayer ? "Player" : "AI"));
        }
    }

    void UpdateCamera()
    {
        if (mainCamera == null)
            return;

        Transform target = isControllingPlayer ? 
            (playerController != null ? playerController.transform : null) : 
            (aiController != null ? aiController.transform : null);

        if (target != null)
        {
            Vector3 targetPosition = target.position + new Vector3(0, cameraHeight, -cameraOffset);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * 5f);
            mainCamera.transform.LookAt(target.position + Vector3.up * 2f);
        }
    }

    public bool IsControllingPlayer()
    {
        return isControllingPlayer;
    }
}

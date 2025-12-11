using UnityEngine;

/// <summary>
/// Quick scene setup tool for testing the game.
/// This creates a basic playable scene at runtime for testing purposes.
/// For production, you should manually set up the scene in the Unity Editor.
/// </summary>
public class QuickSceneSetup : MonoBehaviour
{
    public bool autoSetupOnStart = false;
    public Material defaultMaterial;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }

    [ContextMenu("Setup Test Scene")]
    public void SetupScene()
    {
        Debug.Log("Setting up test scene...");

        // Create ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(3, 1, 3);

        // Create Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.transform.position = new Vector3(0, 0.5f, 0);
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.characterName = "Player";
        pc.moveSpeed = 5f;
        SetColor(player, new Color(0.2f, 0.4f, 1.0f));

        // Create AI
        GameObject ai = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ai.name = "AI";
        ai.transform.position = new Vector3(3, 0.5f, 0);
        AIController ac = ai.AddComponent<AIController>();
        ac.characterName = "AI Helper";
        ac.moveSpeed = 5f;
        SetColor(ai, new Color(1.0f, 0.3f, 0.2f));

        // Create workstations
        CreateWorkStation("Wood", "Wood", new Vector3(-5, 0.5f, 0), Color.yellow);
        CreateWorkStation("Metal", "Metal", new Vector3(-3, 0.5f, 0), Color.gray);
        CreateWorkStation("Fabric", "Fabric", new Vector3(-5, 0.5f, -3), Color.cyan);
        CreateWorkStation("Paint", "Paint", new Vector3(5, 0.5f, 0), Color.magenta);
        CreateWorkStation("Wheels", "Wheels", new Vector3(5, 0.5f, -3), Color.black);
        CreateWorkStation("Stuffing", "Stuffing", new Vector3(-3, 0.5f, -3), Color.white);
        CreateWorkStation("Eyes", "Eyes", new Vector3(3, 0.5f, -3), new Color(0.5f, 0.3f, 0.1f));
        CreateWorkStation("Bow", "Bow", new Vector3(-5, 0.5f, -6), Color.red);
        CreateWorkStation("Circuits", "Circuits", new Vector3(3, 0.5f, -6), Color.green);
        CreateWorkStation("Hair", "Hair", new Vector3(-3, 0.5f, -6), new Color(0.8f, 0.6f, 0.3f));
        CreateWorkStation("Dress", "Dress", new Vector3(5, 0.5f, -6), new Color(1.0f, 0.5f, 0.8f));
        CreateWorkStation("Assembly", "", new Vector3(0, 0.5f, -6), new Color(0.5f, 1.0f, 0.5f));

        // Create managers
        GameObject gmObject = new GameObject("GameManager");
        gmObject.AddComponent<GameManager>();

        GameObject tmObject = new GameObject("ToyManager");
        tmObject.AddComponent<ToyManager>();

        GameObject csObject = new GameObject("CharacterSwapper");
        CharacterSwapper cs = csObject.AddComponent<CharacterSwapper>();
        cs.playerController = pc;
        cs.aiController = ac;
        cs.mainCamera = Camera.main;

        // Setup camera
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0, 10, -8);
            Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        }

        Debug.Log("Test scene setup complete! Press Play to start.");
        Debug.Log("Note: UI Manager needs to be set up manually with Canvas and UI elements.");
    }

    void CreateWorkStation(string stationType, string componentProduced, Vector3 position, Color color)
    {
        GameObject station = GameObject.CreatePrimitive(PrimitiveType.Cube);
        station.name = stationType + " Station";
        station.transform.position = position;
        station.transform.localScale = new Vector3(1.5f, 1, 1.5f);
        
        WorkStation ws = station.AddComponent<WorkStation>();
        ws.stationType = stationType;
        ws.componentProduced = componentProduced;
        ws.interactionTime = 2f;
        
        SetColor(station, color);
    }

    void SetColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public class PropSettings
    {
        public GameObject prefab;
        public int maxAmount = 5;
        public float heightOffset = 0.0f;
        [Range(0, 100)] public float spawnChance = 10f;
        [HideInInspector] public int currentSpawned = 0;
    }

    [Header("Player & Special Prefabs")]
    public GameObject playerPrefab;
    public GameObject giftBoxPrefab;
    public int giftBoxCount = 3;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public List<PropSettings> propPool;
    public Material streetMaterial;

    [Header("Animation Settings")]
    public float spawnDelay = 0.05f;
    public float scaleSpeed = 5.0f;

    [Header("Floor Settings")]
    public int length = 13;
    public int width = 9;
    public float tileSize = 1.0f;

    [Header("Wall Settings")]
    public float wallOffset = 0.5f;
    public float wallHeightOffset = 0.5f;
    public Vector3 wallRotationOffset = new Vector3(0, 90, 0);

    private List<Vector2Int> giftBoxLocations = new List<Vector2Int>();
    private List<Vector2Int> reservedTiles = new List<Vector2Int>();

    void Start()
    {
        GenerateStreet();
        PrecalculateLevelLayout();
        StartCoroutine(GenerateGrid());
    }

    void PrecalculateLevelLayout()
    {
        reservedTiles.Clear();
        giftBoxLocations.Clear();

        for (int x = 0; x <= 1; x++)
        {
            for (int z = 0; z <= 1; z++)
            {
                reservedTiles.Add(new Vector2Int(x, z));
            }
        }

        int placed = 0;
        int attempts = 0;
        while (placed < giftBoxCount && attempts < 100)
        {
            attempts++;
            int rx = Random.Range(2, length - 1);
            int rz = Random.Range(2, width - 1);
            Vector2Int pos = new Vector2Int(rx, rz);

            if (!IsAreaReserved(rx, rz))
            {
                giftBoxLocations.Add(pos);
                for (int x = -1; x <= 1; x++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        reservedTiles.Add(new Vector2Int(rx + x, rz + z));
                    }
                }
                placed++;
            }
        }
    }

    bool IsAreaReserved(int x, int z)
    {
        Vector2Int current = new Vector2Int(x, z);
        foreach (var tile in reservedTiles)
        {
            if (tile == current) return true;
        }
        return false;
    }

    void GenerateStreet()
    {
        GameObject street = GameObject.CreatePrimitive(PrimitiveType.Plane);
        street.name = "Concrete Street";
        street.transform.parent = this.transform;
        street.transform.localPosition = new Vector3(4.5f, -4.2f, 10);
        street.transform.localScale = new Vector3(15, 1f, 15);
        street.layer = LayerMask.NameToLayer("Assets");

        if (streetMaterial != null) street.GetComponent<Renderer>().material = streetMaterial;
    }

    IEnumerator GenerateGrid()
    {
        Quaternion rotationOffset = Quaternion.Euler(wallRotationOffset);
        int layerIndex = LayerMask.NameToLayer("Assets");
        WaitForSeconds delay = new WaitForSeconds(spawnDelay);

        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < width; z++)
            {
                Vector3 floorPos = new Vector3(x * tileSize, 0, z * tileSize);
                SpawnWithScale(floorPrefab, floorPos, Quaternion.identity, layerIndex);

                if (z == 0) SpawnWithScale(wallPrefab, floorPos + new Vector3(0, wallHeightOffset, -wallOffset), Quaternion.Euler(0, 0, 0) * rotationOffset, layerIndex);
                if (z == width - 1) SpawnWithScale(wallPrefab, floorPos + new Vector3(0, wallHeightOffset, wallOffset), Quaternion.Euler(0, 180, 0) * rotationOffset, layerIndex);
                if (x == 0) SpawnWithScale(wallPrefab, floorPos + new Vector3(-wallOffset, wallHeightOffset, 0), Quaternion.Euler(0, 90, 0) * rotationOffset, layerIndex);
                if (x == length - 1) SpawnWithScale(wallPrefab, floorPos + new Vector3(wallOffset, wallHeightOffset, 0), Quaternion.Euler(0, 270, 0) * rotationOffset, layerIndex);

                Vector2Int currentCoord = new Vector2Int(x, z);
                if (giftBoxLocations.Contains(currentCoord))
                {
                    SpawnWithScale(giftBoxPrefab, floorPos, Quaternion.identity, layerIndex);
                }
                else if (!IsAreaReserved(x, z))
                {
                    TrySpawnRandomProp(floorPos, layerIndex);
                }

                yield return delay;
            }
        }

        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        }
    }

    void TrySpawnRandomProp(Vector3 floorPos, int layerIndex)
    {
        int randomIndex = Random.Range(0, propPool.Count);
        PropSettings settings = propPool[randomIndex];

        if (settings.currentSpawned < settings.maxAmount && settings.prefab != null)
        {
            if (Random.Range(0f, 100f) < settings.spawnChance)
            {
                Vector3 propPos = new Vector3(floorPos.x, settings.heightOffset, floorPos.z);
                Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
                SpawnWithScale(settings.prefab, propPos, randomRot, layerIndex);
                settings.currentSpawned++;
            }
        }
    }

    void SpawnWithScale(GameObject prefab, Vector3 pos, Quaternion rot, int layer)
    {
        GameObject obj = Instantiate(prefab, pos, rot, transform);
        obj.layer = layer;
        StartCoroutine(ScaleUpLerp(obj));
    }

    IEnumerator ScaleUpLerp(GameObject target)
    {
        Vector3 targetScale = target.transform.localScale;
        target.transform.localScale = Vector3.zero;
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * scaleSpeed;
            target.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }
        target.transform.localScale = targetScale;
    }
}

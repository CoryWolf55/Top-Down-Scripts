using UnityEngine;
using System.Collections.Generic;

public class ProceduralPropSpawner : MonoBehaviour
{
    [Header("Global Spawn Settings")]
    public int spawnCount = 300;
    public float spawnRadius = 100f;
    public float globalMinDistance = 1.5f;
    public LayerMask groundLayer;

    [Header("Raycast Settings")]
    public float maxRaycastDistance = 300f;
    public float groundOffset = 0.1f;

    [Header("Prop Types")]
    public List<PropType> props = new List<PropType>();

    private List<Vector3> placedPositions = new List<Vector3>();

    private void Start()
    {
        GenerateEnvironment();
    }

    public void GenerateEnvironment()
    {
        int attempts = 0;
        int placed = 0;

        while (placed < spawnCount && attempts < spawnCount * 40)
        {
            attempts++;

            // random point inside radius
            Vector2 r = Random.insideUnitCircle * spawnRadius;
            Vector3 worldPos = new Vector3(r.x, 300f, r.y) + transform.position;

            // drop raycast
            if (!Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, maxRaycastDistance, groundLayer))
            {
                Debug.Log("Fail: Raycast did not hit ground");
                continue;
            }
            
            // choose prop type
            PropType chosen = props[Random.Range(0, props.Count)];

            Vector3 p = new Vector3();
            if (chosen.disableCollider)
                p = hit.point;
            else
                p = Spawner.instance.ChaosSpawn(spawnRadius, spawnCount);

            Vector3 spawnPos = p + Vector3.up * groundOffset;



            // height filtering
            if (spawnPos.y < chosen.minHeight || spawnPos.y > chosen.maxHeight)
            {
                Debug.Log($"Fail: Height {spawnPos.y} outside range {chosen.minHeight} - {chosen.maxHeight}");
                continue;
            }

            // perlin noise check
            if (!PassesNoise(spawnPos, chosen))
            {
                Debug.Log("Fail: Perlin noise below threshold");
                continue;
            }

            // distance check
            if (!IsValidDistance(spawnPos, chosen.minDistance))
            {
                Debug.Log("Fail: Too close to another prop");
                continue;
            }

            // final probability check
            if (Random.value > chosen.spawnChance)
            {
                Debug.Log("Fail: Spawn chance check failed");
                continue;
            }

            // spawn rotation
            Quaternion rot = chosen.allowTilt
                ? Quaternion.Euler(
                    Random.Range(-chosen.tiltAmount, chosen.tiltAmount),
                    Random.Range(0, 360f),
                    Random.Range(-chosen.tiltAmount, chosen.tiltAmount))
                : Quaternion.Euler(0, Random.Range(0, 360f), 0);

           

            GameObject obj = Instantiate(chosen.prefab, spawnPos, rot);

            // random scale
            float scale = Random.Range(chosen.minScale, chosen.maxScale);
            obj.transform.localScale = Vector3.one * scale;

            // tagging
            obj.tag = chosen.isResource ? "Resource" : "Decoration";

            // disable collider if needed
            if (chosen.disableCollider)
            {
                Collider col = obj.GetComponent<Collider>();
                if (col) col.enabled = false;
            }

            // building logic
            if (chosen.isBuilding && scale >= chosen.buildingScaleThreshold)
            {
                if (Spawner.instance != null)
                    Spawner.instance.AddBuilding(obj);
            }

            // track placement
            placedPositions.Add(spawnPos);
            placed++;
        }

        Debug.Log($"Spawned {placed} props after {attempts} attempts.");
    }

    private bool PassesNoise(Vector3 pos, PropType t)
    {
        float x = (pos.x * t.worldScaleFactor + t.noiseOffset.x) * t.noiseScale;
        float z = (pos.z * t.worldScaleFactor + t.noiseOffset.y) * t.noiseScale;

        float v = Mathf.PerlinNoise(x, z);

        Debug.Log($"Noise Value = {v}");

        return v >= t.noiseThreshold;
    }

    private bool IsValidDistance(Vector3 pos, float localMin)
    {
        float required = Mathf.Max(localMin, globalMinDistance);

        foreach (Vector3 p in placedPositions)
        {
            if (Vector3.Distance(p, pos) < required)
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class PropType
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Spawn Chance")]
    public float spawnChance = 1f;

    [Header("Scale Range")]
    public float minScale = 0.8f;
    public float maxScale = 1.6f;

    [Header("Height Limits")]
    public float minHeight = -100f;
    public float maxHeight = 999f;

    [Header("Distance Control")]
    public float minDistance = 1.5f;

    [Header("Perlin Noise")]
    public float noiseScale = 1f;
    public float noiseThreshold = 0.2f;
    public Vector2 noiseOffset;
    public float worldScaleFactor = 0.01f;

    [Header("Rotation & Tilt")]
    public bool allowTilt = true;
    public float tiltAmount = 5f;

    [Header("Special Behavior")]
    public bool isResource = false;
    public bool disableCollider = false;

    [Header("Building Logic")]
    public bool isBuilding = false;
    public float buildingScaleThreshold = 30f;
}

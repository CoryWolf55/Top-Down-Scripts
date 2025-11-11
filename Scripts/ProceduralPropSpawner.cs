using UnityEngine;
using System.Collections.Generic;



public class ProceduralPropSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<PropType> props = new List<PropType>();
    public int spawnCount = 200;
    public float spawnRadius = 100f;
    public float minDistanceBetween = 2f;
    public float isSmallSize = 30f;

    [Header("Height & Grounding")]
    public LayerMask groundLayer;
    public float maxRaycastDistance = 100f;
    public float groundOffset = 0.1f;

    private List<Vector3> placedPositions = new List<Vector3>();

    void Start()
    {
        GenerateEnvironment();
    }

    public void GenerateEnvironment()
    {
        int attempts = 0;
        int placed = 0;

        while (placed < spawnCount && attempts < spawnCount * 10)
        {
            attempts++;

            Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
            Vector3 worldPos = new Vector3(randomPos.x, 100f, randomPos.y) + transform.position;

            // Drop down to ground using raycast
            if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, maxRaycastDistance, groundLayer))
            {
                Vector3 spawnPos = hit.point + Vector3.up * groundOffset;

                // Make sure not too close to another prop
                if (IsPositionValid(spawnPos))
                {
                    // Pick random prop type
                    PropType chosen = props[Random.Range(0, props.Count)];
                    if (Random.value <= chosen.spawnChance)
                    {
                        Quaternion randomTilt = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0, 360f), Random.Range(-5f, 5f));
                        GameObject newProp = Instantiate(chosen.prefab, spawnPos, randomTilt);
                        float scale = Random.Range(chosen.minScale, chosen.maxScale);
                        newProp.transform.localScale = Vector3.one * scale;

                        // Tag for later logic (optional)
                        newProp.tag = chosen.isResource ? "Resource" : "Decoration";
                        

                        placedPositions.Add(spawnPos);
                        if(scale >= isSmallSize)
                        {
                            if (Spawner.instance != null)
                                Spawner.instance.AddBuilding(newProp);
                        }
                        else
                        {
                            newProp.GetComponent<MeshCollider>().enabled = false;
                        }
                        placed++;
                    }
                }
            }
        }

        Debug.Log($"Spawned {placed} environment props after {attempts} attempts.");
    }

    private bool IsPositionValid(Vector3 newPos)
    {
        foreach (Vector3 pos in placedPositions)
        {
            if (Vector3.Distance(pos, newPos) < minDistanceBetween)
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class PropType
{
    public GameObject prefab;
    [Range(0f, 1f)] public float spawnChance = 0.5f; // probability to spawn
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public bool isResource = false; // can be collected or just decoration
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [Header("Loot Settings")]
    public LootTable lootTable;       // Assign a LootTable ScriptableObject in Inspector
    public GameObject lootPrefab;     // Assign the LootPickup prefab in Inspector

    /// <summary>
    /// Call this to spawn loot at a specific world position
    /// </summary>
    public void Drop(Vector3 position)
    {
        if (lootTable == null)
        {
            Debug.LogWarning("LootTable not assigned on " + gameObject.name);
            return;
        }

        if (lootPrefab == null)
        {
            Debug.LogWarning("LootPrefab not assigned on " + gameObject.name);
            return;
        }

        foreach (var entry in lootTable.entries)
        {
            if (Random.value <= entry.dropChance)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                GameObject lootInstance = Instantiate(
                    lootPrefab,
                    position + Random.insideUnitSphere, // scatter slightly
                    Quaternion.identity
                );

                // Initialize the LootPickup
                LootPickup pickup = lootInstance.GetComponent<LootPickup>();
                if (pickup != null)
                    pickup.Initialize(entry.item, amount);
                else
                    Debug.LogError("LootPickup component missing on prefab!");

                // Optional: small physics push
                Rigidbody rb = lootInstance.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
            }
        }
    }
}

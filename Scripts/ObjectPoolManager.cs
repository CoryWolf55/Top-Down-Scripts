using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private Vector3 holdingLocation;

    private Dictionary<string, Pool> pools = new Dictionary<string, Pool>();

    public static ObjectPoolManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public GameObject Spawn(GameObject itemKey, Vector3 location, Quaternion rotation)
    {
        string key = itemKey.name;

        // Get or create pool
        if (!pools.TryGetValue(key, out Pool selected))
        {
            selected = new Pool();
            pools.Add(key, selected);
        }

        // Use pooled object if available
        if (selected.pooledObjects.Count > 0)
        {
            GameObject obj = selected.pooledObjects[0];
            selected.pooledObjects.RemoveAt(0);

            obj.transform.SetPositionAndRotation(location, rotation);

            TrailRenderer trail = obj.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.enabled = false;
                trail.Clear();
                trail.enabled = true;
            }

            obj.SetActive(true);
            return obj;
        }

        // Otherwise make new instance
        GameObject newObj = Instantiate(itemKey, location, rotation);
        return newObj;
    }

    public void Return(GameObject item)
    {

        string key = item.name.Replace("(Clone)", "").Trim();

        if (!pools.TryGetValue(key, out Pool pool))
        {
            pool = new Pool();
            pools.Add(key, pool);
        }

        item.SetActive(false);
        item.transform.position = holdingLocation;

        pool.pooledObjects.Add(item);
    }
}

public class Pool
{
    public List<GameObject> pooledObjects = new List<GameObject>();
}

/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Spawner : MonoBehaviour
{
    #region Variables
    public static Spawner instance; private void Awake() { instance = this; }

    [Header("Attributes")]
    [Range(0f, 1000f)]
    [SerializeField] private float maxWorldHight = 1000;
    [Range(0f, 600f)]
    [SerializeField] private float spawnTimer;
    private float currentSpawnTimer;

    [SerializeField] private bool canSpawn;
    [SerializeField] private List<Transform> spawns;
    [SerializeField] private List<Transform> closeSpawns;
    private Vector3 currentSpawn;
    [Range(0f, 800f)]
    public float maxDistanceFromPlayerToSpawn;
    [Range(0f, 800f)]
    [SerializeField] private float chaosSpawnMaxRange;
    [SerializeField] private bool setSpawn;
    [SerializeField] private bool chaosSpawn;

    [Space]

    [SerializeField] private int amountToSpawn = 1;
    [Range(0, 100)]
    [SerializeField] private int maxNumOfEntitys;
    public List<GameObject> currentEntities;
    private int currentNumOfEntitys;
    [SerializeField] private GameObject[] entityBank;
    [SerializeField] private List<GameObject> allowedEntitys;
    private GameObject entityToSpawn;

    [Space]

    [Range(0f, 500f)]
    [SerializeField] private float minDistanceFromBuildingToSpawn;
    [SerializeField] private string[] playerBuiltStructureNames;
    private List<GameObject> buildings = new List<GameObject>();

    private RaycastHit hit;
    private float shortestDistance = Mathf.Infinity;
    private GameObject nearestBuilding = null;
    private int counter;



    #endregion Variables

    #region normalSpawner
    void Start()
    {
        currentSpawnTimer = spawnTimer;

        BuildUpdate();
    }


    void Update()
    {
        UpdateTimer();
    }

    private void BuildUpdate()
    {
        foreach (string name in playerBuiltStructureNames)
        { 
           // if(name == null)
             //   return;
            GameObject[] b = GameObject.FindGameObjectsWithTag(name);
            foreach (GameObject building in b) 
            {
              //  if(building == null) return;
                buildings.Add(building);
            }
        }
    }

    public void AddBuilding(GameObject building)
    {
        buildings.Add(building);
    }

    private void UpdateTimer()
    {
        if (!canSpawn || currentNumOfEntitys >= maxNumOfEntitys)
            return;
        currentSpawnTimer -= Time.deltaTime;
        if (currentSpawnTimer <= 0)
        {
            GameObject entity = FindRandomEntity();
            Vector3 lLo = FindRandomLocation();
            SpawnEntity(entity, lLo, amountToSpawn);
            currentSpawnTimer = spawnTimer;
        }
    }

    public GameObject FindRandomEntity()
    {
        entityToSpawn = allowedEntitys[Random.Range(0, allowedEntitys.Count)];
        return entityToSpawn;
    }

    public Vector3 FindRandomLocation()
    {
        if (!chaosSpawn)
        {
            IsSpawnCloseEnough();
            if (closeSpawns == null)
                currentSpawn = ChaosSpawn(0,0);
            else
            {
                do { currentSpawn = closeSpawns[Random.Range(0, closeSpawns.Count)].position; }
                while (Vector3.Distance(currentSpawn, PlayerController.instance.transform.position) < maxDistanceFromPlayerToSpawn);
            }

        }
        else
        {
            currentSpawn = ChaosSpawn(0,0);
        }
        return currentSpawn;
    }

    public void SpawnEntity(GameObject entity, Vector3 spawnPoint, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            //GameObject lastEntity = Instantiate(entity, new Vector3(spawnPoint.x, (spawnPoint.y + entity.transform.localScale.y / 2) + 0.5f, spawnPoint.z), Quaternion.identity);
            GameObject lastEntity = ObjectPoolManager.instance.Spawn(entity, new Vector3(spawnPoint.x, (spawnPoint.y + entity.transform.localScale.y / 2) + 0.5f, spawnPoint.z), Quaternion.identity);
            currentEntities.Add(lastEntity);
            currentNumOfEntitys = currentEntities.Count;
            EnemyController ec = lastEntity.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.Init();
            }
           
        }
    }

    public void RemoveEntity(GameObject entity)
    {
        if(currentEntities.Count == 0 || !currentEntities.Contains(entity)) return;
        currentEntities.Remove(entity);
        currentNumOfEntitys = currentEntities.Count;
    }


    void ChangeTimer(float newTime)
    {
        spawnTimer = newTime;
    }

    void AddEntityFromBank(GameObject objectToAdd)
    {
        allowedEntitys.Add(objectToAdd);
    }

    private void IsSpawnCloseEnough()
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            if (Vector3.Distance(spawns[i].position, PlayerController.instance.transform.position) < maxDistanceFromPlayerToSpawn)
                closeSpawns.Add(spawns[i]);
            else if (closeSpawns.Contains(spawns[i]))
                closeSpawns.Remove(spawns[i]);

        }

    }

    public Vector3 ChaosSpawn(float range, int maxAttempts)
    {
        counter = 0;

        if (range == 0)
            range = chaosSpawnMaxRange;
        if(maxAttempts == 0)
            maxAttempts = 100;


        while (true)
        {
            //rays
            Ray ray = new Ray();
            ray.origin = new Vector3(Random.Range(-range, range), maxWorldHight, Random.Range(-range, range));
            ray.direction = Vector3.down;

            Physics.Raycast(ray, out hit, maxWorldHight);
            Debug.DrawRay(ray.origin, ray.direction * maxWorldHight);

            //distance

            shortestDistance = Mathf.Infinity;
            nearestBuilding = null;

            foreach (GameObject building in buildings)
            {
                float distanceToBuilding = Vector3.Distance(hit.point, building.transform.position);

                if (distanceToBuilding < shortestDistance)
                {
                    shortestDistance = distanceToBuilding;
                    nearestBuilding = building;
                }
            }

            if (Vector3.Distance(hit.point, nearestBuilding.transform.position) > minDistanceFromBuildingToSpawn && hit.collider.gameObject.CompareTag("Ground"))
            {
                return hit.point;
            }
            else if (counter >= maxAttempts)
                return spawns[Random.Range(0, spawns.Count)].position;

            counter++;


        }


    }

    public void SetCanSpawn(bool set)
    {
        canSpawn = set;
    }
    public bool CanSpawn()
    {
        return canSpawn;
    }
    #endregion normalSpawner


}
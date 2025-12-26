/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private GameObject Base;

    private NavMeshSurface navSurface;
    public Transform surface;
    private LoadingManager loadingManager;
    [SerializeField] private GameObject loadingManagerPrefab;
    [SerializeField] private GameObject gameContainers;
    private Spawner spawnManager;

    void Awake()
    {
        if (LoadingManager.instance == null)
        {
            Instantiate(loadingManagerPrefab);
        }
        instance = this;
        gameContainers.SetActive(false);
        spawnManager = GetComponent<Spawner>();
        spawnManager.SetCanSpawn(false);
    }

    IEnumerator Start()
    {
        loadingManager = LoadingManager.instance;
        loadingManager.StartLoading(gameObject);
        // Wait a bit so procedural objects can spawn first
        navSurface = surface.GetComponent<NavMeshSurface>();
        if(navSurface == null)
        {
            Debug.LogError("No NavMeshSurface component found on the specified surface Transform.");
            yield break;
        }
        yield return new WaitForSeconds(0.5f);
        BakeNavMesh();
        loadingManager.StopLoading(gameObject);
    }

    public void BakeNavMesh()
    {
        if (surface == null)
        {
            Debug.LogError("No NavMeshSurface found on this GameObject!");
            return;
        }
        //Build the AI Routes
        navSurface.BuildNavMesh();
        FlowfieldGenerator.instance.GenerateFlowfield();
        Debug.Log("NavMesh baked at runtime!");
    }

    public GameObject FindBase()
    {
        return Base;
    }

    public Transform GetSurface()
    {
        return surface;
    }

    public void ActiveGameContainers()
    {
        gameContainers.SetActive(true);
        spawnManager.SetCanSpawn(true);
    }


}
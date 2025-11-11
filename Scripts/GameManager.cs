/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
   

    public NavMeshSurface surface;

    void Awake()
    {
        instance = this;
        
    }

    IEnumerator Start()
    {
        // Wait a bit so procedural objects can spawn first
        yield return new WaitForSeconds(0.5f);
        BakeNavMesh();
    }

    public void BakeNavMesh()
    {
        if (surface == null)
        {
            Debug.LogError("No NavMeshSurface found on this GameObject!");
            return;
        }
        //Build the AI Routes
        surface.BuildNavMesh();
        FlowfieldGenerator.instance.GenerateFlowfield();
        Debug.Log("NavMesh baked at runtime!");
    }


}
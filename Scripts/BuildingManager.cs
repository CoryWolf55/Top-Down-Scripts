/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BuildingManager : MonoBehaviour
{
    public List<GameObject> placedBuildings;

    public BuildingManager instance;
    private void Awake()
    {
        instance = this;
    }
}
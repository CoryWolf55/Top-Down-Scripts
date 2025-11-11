using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingPlacement : MonoBehaviour
{
    public static BuildingPlacement instance;

    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 2f;
    public bool showGrid = true;

    [Header("Materials")]
    public Material correctPlacementMat;
    public Material incorrectPlacementMat;

    [Header("Building")]
    public BuildingData buildingToPlace;

    private GameObject currentPreview;
    private bool buildMode = false;
    private bool canPlace = true;
    private Vector3 previewPosition;
    private GameObject projected;
    private bool beenProjected = false;
    private float rotationY = 0f; // Rotation of preview

    private void Awake() => instance = this;

    private void Start()
    {
        // Instantiate preview but hide it
        currentPreview = Instantiate(buildingToPlace.prefab, Vector3.up * 100, Quaternion.identity);
        //currentPreview.GetComponent<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        HandleInput();
        if (!buildMode) return;

        HandleRotation();
        UpdatePreviewPosition();
        HandlePlacement();

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceBuilding();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildMode = !buildMode;
            EnableMesh(buildMode);

            // Hide preview if build mode turned off
            if (!buildMode && projected != null)
            {
                Destroy(projected);
                beenProjected = false;
            }
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationY += 90f;
            if (rotationY >= 360f) rotationY = 0f;

            if (projected != null)
                projected.transform.rotation = Quaternion.Euler(0, rotationY, 0);
        }
    }

    private void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            // Snap to grid
            Vector3 snapped = new Vector3(
                Mathf.Round(worldPoint.x / cellSize) * cellSize,
                buildingToPlace.prefab.transform.localScale.y / 2,
                Mathf.Round(worldPoint.z / cellSize) * cellSize
            );

            previewPosition = snapped;

            if (beenProjected && projected != null)
            {
                projected.transform.position = previewPosition;
                projected.transform.rotation = Quaternion.Euler(0, rotationY, 0);
            }
        }
    }

    private void HandlePlacement()
    {
        if (!beenProjected)
        {
            SetMesh(correctPlacementMat);
            projected = Instantiate(currentPreview, previewPosition, Quaternion.Euler(0, rotationY, 0));
            projected.GetComponent<Collider>().isTrigger = true;
            beenProjected = true;

            if (!projected.TryGetComponent(out ValidPlacementCheck vpc))
            {
                vpc = projected.AddComponent<ValidPlacementCheck>();
                vpc.BP = this;
            }
        }
    }

    public void OnTrigger(bool trigger)
    {
        canPlace = !trigger;
        SetMesh(trigger ? incorrectPlacementMat : correctPlacementMat);
    }

    private void PlaceBuilding()
    {
        if (!canPlace) return;

        GameObject newBuilding = Instantiate(buildingToPlace.prefab, previewPosition, Quaternion.Euler(0, rotationY, 0));
        Collider col = newBuilding.GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        if (newBuilding.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = newBuilding.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;
        }

        NavMeshObstacle obs = newBuilding.AddComponent<NavMeshObstacle>();
        obs.carving = true;
        obs.carveOnlyStationary = true;

        beenProjected = false;
        Destroy(projected);

        // Update Flowfield
        if (FlowfieldGenerator.instance != null)
            FlowfieldGenerator.instance.UpdateFlowfieldArea(newBuilding.transform.position,0f);

        if (Spawner.instance != null)
            Spawner.instance.AddBuilding(newBuilding);
    }

    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = Color.grey;
        for (int x = 0; x <= gridWidth; x++)
        {
            Gizmos.DrawLine(new Vector3(x * cellSize, 0, 0), new Vector3(x * cellSize, 0, gridHeight * cellSize));
        }
        for (int z = 0; z <= gridHeight; z++)
        {
            Gizmos.DrawLine(new Vector3(0, 0, z * cellSize), new Vector3(gridWidth * cellSize, 0, z * cellSize));
        }
    }

    private void SetMesh(Material mat)
    {
        foreach(Transform child in currentPreview.transform)
        {
            child.GetComponent<Renderer>().material = mat;
        }
    }

    private void EnableMesh(bool enable)
    {
        foreach(Transform child in currentPreview.transform)
        {
            child.GetComponent<Renderer>().enabled = enable;
        }
    }
}

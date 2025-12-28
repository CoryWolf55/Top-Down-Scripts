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

    [Header("Grid Visuals")]
    public float gridYOffset = 0.01f;
    public float lineWidth = 0.05f;

    [Header("Cell Materials")]
    public Material occupiedCellMat;

    [Header("Placement Materials")]
    public Material correctPlacementMat;
    public Material incorrectPlacementMat;

    [Header("Building")]
    public BuildingData buildingToPlace;

    private Transform playerTransform;

    private GameObject currentPreview;
    private GameObject projected;
    private GameObject runtimeGrid;

    private bool buildMode;
    private bool canPlace;
    private bool beenProjected;

    private Vector3 previewPosition;
    private float rotationY;

    private Vector3 gridOrigin;

    private Dictionary<Vector2Int, GameObject> cellFills = new();
    private HashSet<Vector2Int> occupiedCells = new();

    private void Awake() => instance = this;

    private void Start()
    {
        LoadingManager.instance.StartLoading(gameObject);
        playerTransform = transform;

        currentPreview = Instantiate(buildingToPlace.prefab, Vector3.up * 100f, Quaternion.identity);
        EnableMesh(false);

        gridOrigin = playerTransform.position -
                     new Vector3(gridWidth * cellSize / 2f, 0f, gridHeight * cellSize / 2f);

        CreateRuntimeGrid();
        runtimeGrid.SetActive(false);
        LoadingManager.instance.StopLoading(gameObject);
    }

    private void Update()
    {
        HandleInput();
        if (!buildMode) return;

        HandleRotation();
        UpdatePreviewPosition();
        HandlePreviewSpawn();

        if (Input.GetMouseButtonDown(0) && canPlace)
            PlaceBuilding();
    }

    /* ================= INPUT ================= */

    private void HandleInput()
    {
        if (!Input.GetKeyDown(KeyCode.B)) return;

        buildMode = !buildMode;
        EnableMesh(buildMode);

        runtimeGrid.SetActive(buildMode);

        if (!buildMode && projected != null)
        {
            Destroy(projected);
            beenProjected = false;
        }
    }

    private void HandleRotation()
    {
        if (!Input.GetKeyDown(KeyCode.R)) return;

        rotationY = (rotationY + 90f) % 360f;

        if (projected != null)
            projected.transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    /* ================= PREVIEW ================= */

    private void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (!plane.Raycast(ray, out float distance)) return;

        Vector3 worldPoint = ray.GetPoint(distance);
        float halfCell = cellSize / 2f;

        previewPosition = new Vector3(
            Mathf.Floor((worldPoint.x - gridOrigin.x) / cellSize) * cellSize + halfCell + gridOrigin.x,
            buildingToPlace.prefab.transform.localScale.y / 2f,
            Mathf.Floor((worldPoint.z - gridOrigin.z) / cellSize) * cellSize + halfCell + gridOrigin.z
        );

        Vector2Int originCell = WorldToCell(previewPosition);
        List<Vector2Int> footprint = GetFootprintCells(originCell);

        canPlace = true;
        foreach (var cell in footprint)
        {
            if (occupiedCells.Contains(cell))
            {
                canPlace = false;
                break;
            }
        }

        if(canPlace)
            SetMesh(correctPlacementMat);
        else
            SetMesh(incorrectPlacementMat);

        if (projected != null)
        {
            projected.transform.position = previewPosition + GetFootprintOffset();
            projected.transform.rotation = Quaternion.Euler(0, rotationY, 0);
        }
    }

    private void HandlePreviewSpawn()
    {
        if (beenProjected) return;

        projected = Instantiate(
            currentPreview,
            previewPosition + GetFootprintOffset(),
            Quaternion.Euler(0, rotationY, 0)
        );

        beenProjected = true;
    }

    /* ================= PLACEMENT ================= */

    private void PlaceBuilding()
    {
        Vector2Int originCell = WorldToCell(previewPosition);
        List<Vector2Int> footprint = GetFootprintCells(originCell);

        foreach (var cell in footprint)
            if (occupiedCells.Contains(cell))
                return;

        GameObject newBuilding = Instantiate(
            buildingToPlace.prefab,
            previewPosition + GetFootprintOffset(),
            Quaternion.Euler(0, rotationY, 0)
        );

        Rigidbody rb = newBuilding.GetComponent<Rigidbody>() ?? newBuilding.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;

        NavMeshObstacle obs = newBuilding.AddComponent<NavMeshObstacle>();
        obs.carving = true;
        obs.carveOnlyStationary = true;

        foreach (var cell in footprint)
        {
            occupiedCells.Add(cell);

            if (cellFills.TryGetValue(cell, out GameObject fill))
                fill.SetActive(true);
        }

        Destroy(projected);
        beenProjected = false;

        if (FlowfieldGenerator.instance != null)
            FlowfieldGenerator.instance.UpdateFlowfieldArea(newBuilding.transform.position, 0f);

        if (Spawner.instance != null)
            Spawner.instance.AddBuilding(newBuilding);
    }

    /* ================= GRID ================= */

    private void CreateRuntimeGrid()
    {
        runtimeGrid = new GameObject("RuntimeGrid");
        cellFills.Clear();

        for (int x = 0; x <= gridWidth; x++)
        {
            CreateLine(
                gridOrigin + new Vector3(x * cellSize, gridYOffset, 0),
                gridOrigin + new Vector3(x * cellSize, gridYOffset, gridHeight * cellSize)
            );
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            CreateLine(
                gridOrigin + new Vector3(0, gridYOffset, z * cellSize),
                gridOrigin + new Vector3(gridWidth * cellSize, gridYOffset, z * cellSize)
            );
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 pos = gridOrigin + new Vector3(
                    x * cellSize + cellSize / 2f,
                    gridYOffset,
                    z * cellSize + cellSize / 2f
                );

                GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fill.transform.SetParent(runtimeGrid.transform);
                fill.transform.position = pos;
                fill.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                fill.transform.localScale = Vector3.one * cellSize;

                Destroy(fill.GetComponent<Collider>());
                fill.GetComponent<Renderer>().material = occupiedCellMat;
                fill.SetActive(false);

                cellFills[new Vector2Int(x, z)] = fill;
            }
        }
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.SetParent(runtimeGrid.transform);

        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.widthMultiplier = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    /* ================= HELPERS ================= */

    private Vector2Int WorldToCell(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize),
            Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize)
        );
    }

    private Vector2Int GetRotatedFootprint()
    {
        if (rotationY == 90f || rotationY == 270f)
            return new Vector2Int(
                buildingToPlace.footprint.y,
                buildingToPlace.footprint.x
            );

        return buildingToPlace.footprint;
    }

    private List<Vector2Int> GetFootprintCells(Vector2Int origin)
    {
        List<Vector2Int> cells = new();
        Vector2Int size = GetRotatedFootprint();

        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                cells.Add(new Vector2Int(origin.x + x, origin.y + z));

        return cells;
    }

    private Vector3 GetFootprintOffset()
    {
        Vector2Int size = GetRotatedFootprint();
        return new Vector3(
            (size.x * cellSize) / 2f - cellSize / 2f,
            0f,
            (size.y * cellSize) / 2f - cellSize / 2f
        );
    }

    private void SetMesh(Material mat)
    {
        if (projected == null) return;

        foreach (Transform child in projected.transform)
        {
            Renderer rend = child.GetComponent<Renderer>();
            if (rend != null)
                rend.material = mat;
        }
    }

    private void EnableMesh(bool enable)
    {
        foreach (Transform child in currentPreview.transform)
            child.GetComponent<Renderer>().enabled = enable;
    }
}

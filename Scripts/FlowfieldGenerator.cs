using UnityEngine;
using UnityEngine.AI;


public class FlowfieldGenerator : MonoBehaviour
{
    public static FlowfieldGenerator instance;


    [Header("Flowfield Settings")]
    public Transform target; // Base/drill
    public int gridSizeX = 50;
    public int gridSizeZ = 50;
    public float cellSize = 2f;

    private Vector3[,] directions;
    private Vector3 mapOrigin;

    private void Awake()
    {
        instance = this;
       
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject baseObj = GameObject.FindWithTag("Base");
            if (baseObj != null) target = baseObj.transform;
        }
        if(gridSizeX == 0  || gridSizeZ == 0)
        {
            gridSizeX = (int)Spawner.instance.maxDistanceFromPlayerToSpawn;
            gridSizeZ = (int)Spawner.instance.maxDistanceFromPlayerToSpawn;
        }
        ComputeMapOrigin();
        
    }

    private void ComputeMapOrigin()
    {
        float originX = -((gridSizeX - 1) * cellSize) / 2f;
        float originZ = -((gridSizeZ - 1) * cellSize) / 2f;
        mapOrigin = new Vector3(originX, 0f, originZ);
    }

    public void GenerateFlowfield()
    {
        directions = new Vector3[gridSizeX, gridSizeZ];

        for (int x = 0; x < gridSizeX; x++)
            for (int z = 0; z < gridSizeZ; z++)
                UpdateCell(x, z);
    }

    public void UpdateFlowfieldArea(Vector3 position, float radius)
    {
        if(radius == 0f) radius = 5f;
        Vector2Int centerCell = WorldPosToCell(position);
        int radiusInCells = Mathf.CeilToInt(radius / cellSize);

        for (int x = centerCell.x - radiusInCells; x <= centerCell.x + radiusInCells; x++)
        {
            for (int z = centerCell.y - radiusInCells; z <= centerCell.y + radiusInCells; z++)
            {
                if (x < 0 || x >= gridSizeX || z < 0 || z >= gridSizeZ) continue;
                UpdateCell(x, z);
            }
        }
    }

    private void UpdateCell(int x, int z)
    {
        Vector3 worldPos = mapOrigin + new Vector3(x * cellSize, 0f, z * cellSize);
        NavMeshPath path = new NavMeshPath();

        if (target != null && NavMesh.CalculatePath(worldPos, target.position, NavMesh.AllAreas, path) && path.corners.Length > 1)
        {
            directions[x, z] = (path.corners[1] - worldPos).normalized;
        }
        else
        {
            directions[x, z] = Vector3.zero;
        }
    }

    public Vector2Int WorldPosToCell(Vector3 worldPos)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt((worldPos.x - mapOrigin.x) / cellSize), 0, gridSizeX - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt((worldPos.z - mapOrigin.z) / cellSize), 0, gridSizeZ - 1);
        return new Vector2Int(x, z);
    }

    public Vector3 GetDirection(Vector3 worldPos)
    {
        Vector2Int cell = WorldPosToCell(worldPos);

        // check neighbors for a valid direction
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = Mathf.Clamp(cell.x + dx, 0, gridSizeX - 1);
                int nz = Mathf.Clamp(cell.y + dz, 0, gridSizeZ - 1);
                if (directions[nx, nz].sqrMagnitude > 0.001f)
                    return directions[nx, nz];
            }

        // fallback to direct vector
        if (target != null)
            return (target.position - worldPos).normalized;

        return Vector3.zero;
    }

    

    private void OnDrawGizmos()
    {
        if (directions == null) return;

        Gizmos.color = Color.cyan;
        for (int x = 0; x < gridSizeX; x++)
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 pos = mapOrigin + new Vector3(x * cellSize, 0f, z * cellSize);
                Vector3 dir = directions[x, z];
                if (dir.sqrMagnitude > 0.001f)
                    Gizmos.DrawLine(pos, pos + dir);
            }
    }
}

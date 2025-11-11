using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyBrain : EnemyBrain
{
    [Header("Detection & Wall Logic")]
    public float playerDetectionRange = 10f;
    public float attackRange = 1.5f;
    public float wallCheckDistance = 1.5f;
    public float wallBreakThreshold = 50f;

    public float breakVsWalkWeight = 0.5f; // 0 = prefer walk around, 1 = prefer breaking
    public LayerMask playerLayer;
    public LayerMask wallLayer;

    private GameObject currentTargetWall;
    private bool isBreakingWall;
    private bool isChasingPlayer;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<EnemyController>();

        if (target == null)
            target = GameObject.FindGameObjectWithTag("Base")?.transform;
    }

    public override Vector3? GetTargetLocation()
    {
        if (target != null)
            return target.position;
        return null;
    }

    public override bool CanBreakObstacle(GameObject obstacle)
    {
        Wall wall = obstacle.GetComponent<Wall>();
        if (wall != null)
        {
            float weight = Mathf.InverseLerp(0, wall.maxHealth, wall.currentHealth);
            return weight <= breakVsWalkWeight;
        }
        return false;
    }

    public override void BreakObstacle(GameObject obstacle)
    {
        Wall wall = obstacle.GetComponent<Wall>();
        if (wall != null)
        {
            controller.StopMoving();
            controller.Attack(obstacle);
        }
    }

    private void Update()
    {
        if (controller == null) return;

        Vector3 targetPos = target != null ? target.position : transform.position;
        isBreakingWall = false;
        isChasingPlayer = false;
        currentTargetWall = null;

        // Detect player
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, playerDetectionRange, playerLayer);
        if (hitPlayers.Length > 0)
        {
            isChasingPlayer = true;
            targetPos = hitPlayers[0].transform.position;
        }

        // Detect walls in front
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
            (targetPos - transform.position).normalized,
            out RaycastHit hit,
            wallCheckDistance,
            wallLayer))
        {
            currentTargetWall = hit.collider.gameObject;
            Wall wall = hit.collider.GetComponent<Wall>();

            if (wall != null && CanBreakObstacle(currentTargetWall))
            {
                isBreakingWall = true;
                controller.StopMoving();
                controller.Attack(currentTargetWall);
                controller.flowField.UpdateFlowfieldArea(currentTargetWall.transform.position, 2f);
                return;
            }
            else
            {
                // Walk around using navmesh
                NavMeshPath path = new NavMeshPath();
                if (controller.agent.CalculatePath(targetPos, path) &&
                    path.status == NavMeshPathStatus.PathComplete &&
                    path.corners.Length > 1)
                {
                    controller.StartMoving(path.corners[1]);
                    return;
                }
            }
        }

        // Attack player
        if (isChasingPlayer && Vector3.Distance(transform.position, targetPos) <= attackRange)
        {
            controller.StopMoving();
            controller.Attack(hitPlayers[0].gameObject);
        }
        else
        {
            controller.StartMoving(targetPos);
        }
    }
}

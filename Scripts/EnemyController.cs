using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public EnemyData enemyDataStatic;
    public float navSwitchDistance = 2f;
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 1.5f;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public FlowfieldGenerator flowField;
    private Rigidbody rb;
    private EnemyBrain brain;

    public EnemyData enemyData;
    private float currentHealth;
    private float speed;
    private float rotateSpeed;

    private Vector3 targetPosition;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    [SerializeField] private float stuckThreshold = 1f;
    [SerializeField] private float moveThreshold = 0.05f;

    private Vector3 lastMoveDir = Vector3.zero;
    private bool usingNavMesh = false;
    private bool obstacleDetected = false;
    [SerializeField]
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY;

        brain = GetComponent<EnemyBrain>();
        enemyData = enemyDataStatic;

        speed = enemyData.speed;
        rotateSpeed = enemyData.turnSpeed;
        currentHealth = enemyData.health;
    }

    void Start()
    {
        flowField = FlowfieldGenerator.instance;
        if (flowField == null)
            Debug.LogWarning("Flowfield not assigned!");
    }

    void FixedUpdate()
    {
        if (targetPosition == Vector3.zero) return;

        agent.nextPosition = transform.position;
        Vector3 moveDir = Vector3.zero;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        usingNavMesh = distanceToTarget <= navSwitchDistance;
        obstacleDetected = false;

        // Flowfield movement if far
        if (!usingNavMesh && flowField != null)
        {
            moveDir = flowField.GetDirection(transform.position);
            if (moveDir == Vector3.zero)
            {
                moveDir = (targetPosition - transform.position).normalized;
            }
        }

        // Obstacle detection
        if (moveDir.sqrMagnitude > 0.001f)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, moveDir, out RaycastHit hit, obstacleCheckDistance, obstacleLayer))
            {
                obstacleDetected = true;

                if (brain != null && brain.CanBreakObstacle(hit.collider.gameObject))
                {
                    // Attack wall
                    rb.linearVelocity = Vector3.zero;
                    brain.BreakObstacle(hit.collider.gameObject);
                    flowField.UpdateFlowfieldArea(hit.collider.transform.position, 2f);
                    moveDir = Vector3.zero;
                }
                else
                {
                    // Walk around using NavMesh
                    NavMeshPath path = new NavMeshPath();
                    if (agent.CalculatePath(targetPosition, path) && path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1)
                    {
                        moveDir = (path.corners[1] - transform.position).normalized;
                    }
                    else
                    {
                        // fallback direct move
                        moveDir = (targetPosition - transform.position).normalized;
                    }
                }
            }
        }

        // NavMesh movement near target
        if (usingNavMesh)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPosition, path) && path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1)
            {
                moveDir = (path.corners[1] - transform.position).normalized;
            }
            else
            {
                moveDir = (targetPosition - transform.position).normalized;
            }
        }

        // Stuck detection
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        if (distanceMoved < moveThreshold)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        if (stuckTimer >= stuckThreshold)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPosition, path) && path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1)
            {
                moveDir = (path.corners[1] - transform.position).normalized;
            }
            stuckTimer = 0f;
        }

        lastPosition = transform.position;

        // Rigidbody movement
        if (moveDir.sqrMagnitude > 0.001f)
        {
            rb.linearVelocity = new Vector3(moveDir.x, 0, moveDir.z) * speed;
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDir), rotateSpeed);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }

        lastMoveDir = moveDir;
    }

    #region Brain Functions
    public void StartMoving(Vector3 target)
    {
        targetPosition = target;
    }

    public void StopMoving()
    {
        targetPosition = transform.position;
        rb.linearVelocity = Vector3.zero;
    }

    public void Attack(GameObject target)
    {
        Wall wall = target.GetComponent<Wall>();
        if (wall != null)
        {
            wall.TakeDamage(enemyData.damage);
        }

        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(enemyData.damage);
        }
    }
    #endregion

    public void TakeDamage(float damage)
    {
        if (enemyData != null)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        Spawner.instance.RemoveEntity(gameObject);
        GetComponent<LootDrop>().Drop(transform.position);
        Destroy(gameObject);
    }

    public void Init()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyData = enemyDataStatic;
        speed = enemyData.speed;
        rotateSpeed = enemyData.turnSpeed;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + lastMoveDir);
        if (obstacleDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, lastMoveDir * obstacleCheckDistance);
        }
    }
}

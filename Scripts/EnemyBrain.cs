using UnityEngine;

public abstract class EnemyBrain : MonoBehaviour
{
    protected EnemyController controller;
    protected Transform target;

    protected virtual void Awake()
    {
        controller = GetComponent<EnemyController>();
    }

    // Get where this enemy should try to move
    public abstract Vector3? GetTargetLocation();

    // Should this enemy break this obstacle?
    public abstract bool CanBreakObstacle(GameObject obstacle);

    // How does this enemy break the obstacle?
    public abstract void BreakObstacle(GameObject obstacle);
}

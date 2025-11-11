using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ItemMagnet))]
public class DroneController : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 10f;
    public float hoverHeight = 2f;
    public float hoverForce = 5f;
    public float stabilityForce = 2f;
    public float damping = 2f;
    public float stopSmooth = 3f;

    [Header("Loot Collection")]
    public float scanRadius = 10f;
    private float pickupDistance = 1.2f;
    public float rescanInterval = 2f;

    private Rigidbody rb;
    private LootPickup targetLoot;
    private bool hasTarget = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pickupDistance = GetComponent<ItemMagnet>().pickupRadius;
        rb.useGravity = false;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 4f;

        StartCoroutine(ScanForLoot());
    }

    void FixedUpdate()
    {
        MaintainHover();

        if (hasTarget && targetLoot != null)
        {
            MoveToTarget();
            FaceMovementDirection(); 
        }
        else
        {
            StabilizeAndSlow();
        }
    }

    void FaceMovementDirection()
    {
        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0;

        if (horizontalVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    void MaintainHover()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight + 1f))
        {
            float heightError = hoverHeight - hit.distance;
            float lift = heightError * hoverForce - rb.linearVelocity.y * damping;
            rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);
        }

        // keep upright
        Quaternion uprightRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * rb.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, uprightRotation, stabilityForce * Time.fixedDeltaTime));
    }

    void MoveToTarget()
    {
        if (targetLoot == null)
        {
            hasTarget = false;
            return;
        }

        Vector3 direction = (targetLoot.transform.position - transform.position);
        direction.y = 0; // only horizontal movement

        // Apply thrust toward loot
        rb.AddForce(direction.normalized * moveForce, ForceMode.Acceleration);

        // If close enough, collect
        if (direction.magnitude <= pickupDistance)
        {
            CollectLoot(targetLoot);
        }
    }

    void CollectLoot(LootPickup loot)
    {
        targetLoot = null;
        hasTarget = false;
        StopMovement();
    }

    void StabilizeAndSlow()
    {
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, stopSmooth * Time.fixedDeltaTime);
    }

    IEnumerator ScanForLoot()
    {
        while (true)
        {
            if (!hasTarget)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius);
                float closestDist = Mathf.Infinity;
                LootPickup closestLoot = null;

                foreach (var hit in hits)
                {
                    LootPickup loot = hit.GetComponent<LootPickup>();
                    if (loot != null && loot.BeingPickedUp() == false)
                    {
                        
                        float dist = Vector3.Distance(transform.position, loot.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestLoot = loot;
                            loot.Claim(true);
                        }
                    }
                }

                if (closestLoot != null)
                {
                    targetLoot = closestLoot;
                    hasTarget = true;
                    
                }
            }

            yield return new WaitForSeconds(rescanInterval);
        }
    }

    public void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }

}

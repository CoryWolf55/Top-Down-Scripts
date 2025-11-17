using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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
    [SerializeField] private float pickupDistance = 1.2f;
    public float rescanInterval = 2f;

    private Rigidbody rb;
    private LootPickup targetLoot;
    private bool hasTarget = false;

    private GameObject tube;
    private Transform holdingSpot;
    [SerializeField] private float animationSpeed = 2f;

    private Coroutine scanRoutine;

    [Header("Base")]
    private Transform baseLocation; //Set this during placement of drone
    private bool returningToBase = false;

    private LayerMask groundMask;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 4f;

        //Grab children components
        tube = transform.Find("Tube").gameObject;
        holdingSpot = transform.Find("HoldingSpot");
        groundMask = LayerMask.GetMask("Ground");

        //base
        

        scanRoutine = StartCoroutine(ScanForLoot());
    }

    private void Start()
    {
        if (baseLocation == null)
        {
            baseLocation = GameManager.instance.FindBase().transform;
        }
    }
    

    void FixedUpdate()
    {
        MaintainHover();

        if (hasTarget && targetLoot != null)
        {
            MoveToTarget();
            FaceMovementDirection(); 
        }
        if(returningToBase && baseLocation != null)
        {
            MoveToBase();
            FaceMovementDirection();
        }
        else
        {
            StabilizeAndSlow();
        }
    }

    void MoveToBase()
    {
        Vector3 direction = (baseLocation.position - transform.position);
        direction.y = 0; // only horizontal movement
        // Apply thrust toward base
        rb.AddForce(direction.normalized * moveForce, ForceMode.Acceleration);

        if (direction.magnitude <= pickupDistance)
        {
            //Drop Loot
            returningToBase = false;
            GameObject collectedLoot = holdingSpot.GetChild(0).gameObject;
            collectedLoot.transform.SetParent(null);
            Rigidbody lootRb = collectedLoot.GetComponent<Rigidbody>();
            if (lootRb == null)
            {
                lootRb = collectedLoot.AddComponent<Rigidbody>();
            }
            lootRb.useGravity = true;
            collectedLoot.GetComponent<Collider>().isTrigger = false;

            //Return to searching for loot
            scanRoutine = StartCoroutine(ScanForLoot());
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
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight + 1f, groundMask))
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
            StartCoroutine(CollectLoot(targetLoot));
        }
    }

    IEnumerator CollectLoot(LootPickup loot)
    {
        
        hasTarget = false;
        StopMovement();

        targetLoot = null;
        tube.GetComponent<MeshRenderer>().enabled = true;
        Debug.Log("Starting collection animation.");
        while (true)
        {

            Vector3 scale = new Vector3(tube.transform.localScale.x, this.transform.position.y * 2, tube.transform.localScale.z);
            Vector3 newScale = Vector3.Lerp(tube.transform.localScale, scale, Time.deltaTime * animationSpeed);
            Vector3 pos = new Vector3(tube.transform.position.x, this.transform.position.y / 2, tube.transform.position.z);

            tube.transform.position = Vector3.Lerp(tube.transform.position, pos, Time.deltaTime * animationSpeed);

            tube.transform.localScale = newScale;

            if (Mathf.Abs(tube.transform.localScale.y - scale.y) < 0.01f)
            {
                tube.transform.localScale = scale;
                Debug.Log("Animation complete.");
                break;
            }

            yield return null;
        }

        // Attach loot to holding spot and shrink tube
        if (loot.gameObject.GetComponent<Rigidbody>() != null)
        {
            loot.gameObject.GetComponent<Rigidbody>().useGravity = false;
        }

        while (true)
        {
            float holdingY = holdingSpot.position.y - loot.transform.localScale.y / 2;
            Vector3 holdingLocation = new Vector3(holdingSpot.position.x, holdingY, holdingSpot.position.z);
            loot.transform.position = Vector3.Lerp(loot.transform.position, holdingLocation, Time.deltaTime * animationSpeed);
            loot.transform.rotation = Quaternion.Slerp(loot.transform.rotation, holdingSpot.rotation, Time.deltaTime * animationSpeed);


            //Shrink Tube
            
            Vector3 scale = new Vector3(tube.transform.localScale.x, 0, tube.transform.localScale.z);
            Vector3 newScale = Vector3.Lerp(tube.transform.localScale, scale, Time.deltaTime * animationSpeed);
            Vector3 pos = new Vector3(tube.transform.position.x, transform.position.y, tube.transform.position.z);

            tube.transform.position = Vector3.Lerp(tube.transform.position, pos, Time.deltaTime * animationSpeed);

            tube.transform.localScale = newScale;

            if (Mathf.Abs(tube.transform.localScale.y - scale.y) < 0.01f)
            {
                tube.transform.localScale = scale;
                Debug.Log("Animation complete.");
                tube.GetComponent<MeshRenderer>().enabled = false;
                
            }


            // Check if loot is in position then proceeed
            if (Mathf.Abs(loot.transform.position.y - holdingY) < 0.01f)
            {
                loot.transform.position = holdingLocation;
                loot.transform.rotation = holdingSpot.rotation;

                loot.transform.SetParent(holdingSpot);

                loot.gameObject.tag = "CollectedLoot";

                Debug.Log("Loot secured.");
                break;
            }
            yield return null;
        }

        //Loot is being held and secured, ready for next target
        
        
            FindBase();
            returningToBase = true;
            yield break;
        
      

    }

    void FindBase()
    {
       baseLocation = GameManager.instance.FindBase().transform;
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
        if (scanRoutine != null)
        {
            StopCoroutine(scanRoutine);
            scanRoutine = null;
        }
    }

   


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }

}

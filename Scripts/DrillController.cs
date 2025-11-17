using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrillController : MonoBehaviour
{
    //Script for controlling drill rotation animation and drops

    [Header("Power Settings")]
    private bool hasPower = true;
    private int powerUse = 10;

    [Header("Animation Settings")]
    private Coroutine startAnimation;
    private Transform drillBit;
    private Transform drillBitHolder;
    [SerializeField] private float distanceFromOre;
    [SerializeField] private float droppingSpeed = 1.5f;
    [SerializeField] private float rotSpeed = 2f;
    [SerializeField] private float speedupSpeed = 0.1f;
    [SerializeField] private float outPutForce = 1f;

    [Header("Drill Settings")]
    [SerializeField] private int outputSpeed;
    private Transform outputLocation;
    [SerializeField] LayerMask oreMask;
    private GameObject ore;


    [Header("Drop Settings")]
     private OreData oreData;
     private ItemData itemDropped;
    private GameObject lootPrefab;


    void Awake()
    {
        drillBit = FindDeepChild(transform, "DrillBit");
        drillBitHolder = FindDeepChild(transform, "DrillBitHolder");
        outputLocation = FindDeepChild(transform, "OutputSpot");
    }

    void Start()
    {
        ActivateDrill();
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    void ActivateDrill()
    {
        if(PowerManager.instance.PowerAvailability(powerUse))
        {
            hasPower = true;
            PowerManager.instance.UsePower(powerUse);
            if(startAnimation == null)
            {
                startAnimation = StartCoroutine(DrillAnimation());
            }
        }
        else
        {
            
            hasPower = false;
            PowerManager.instance.StopUsingPower(powerUse);
            if (startAnimation == null)
                return;
            StopAllCoroutines();
            //Stop drill animation
        }
    }


    private IEnumerator DrillAnimation()
    {
        //Animations for drill rotation
        Ray ray = new Ray(drillBit.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f, oreMask))
        {
            ore = hit.collider.gameObject;
        }
        else { Debug.Log("There was no ore"); yield break; }

        //Setup ore and drops
        oreData = ore.GetComponent<OreNodeController>().oreData;
        itemDropped = oreData.output;


        int count = 0;
            while (true) //Setting drill into ore
            {
                float targetY = (ore.transform.position.y + distanceFromOre);
                Vector3 pos = new Vector3(drillBitHolder.position.x,targetY, drillBitHolder.position.z);
                drillBitHolder.position = Vector3.Lerp(drillBitHolder.position, pos, Time.deltaTime * droppingSpeed);

                if (Mathf.Abs(drillBitHolder.position.y - targetY) < 0.01f)
                {
                    drillBitHolder.position = pos;
                    Debug.Log("Animation complete.");
                    break;
                }
              
                 yield return null;
            }

        
        StartCoroutine(Rotating());
        yield return new WaitForSeconds(1f);
        StartCoroutine(Mining());


        yield break;
    }


    IEnumerator Rotating()
    {
        float currentSpeed = 0;
        while (true) //Starting Rotation
        {
            currentSpeed = Mathf.Lerp(currentSpeed, rotSpeed, Time.deltaTime * speedupSpeed);
            drillBit.Rotate(Vector3.forward * currentSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator Mining()
    {
        while(true)
        {

            int outPutAmmount = Random.Range(oreData.minOreOutput, oreData.maxOreOutput);
            for (int i = 0; i < outPutAmmount; i++)
            {
                GameObject lastSpawned = ObjectPoolManager.instance.Spawn(itemDropped.prefab, outputLocation.position, Quaternion.identity);
                Rigidbody rb = lastSpawned.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(outputLocation.forward * outPutForce, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(outputSpeed);
        }
    }




}

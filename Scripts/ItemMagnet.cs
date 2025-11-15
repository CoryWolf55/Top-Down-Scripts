using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMagnet : MonoBehaviour
{
   
    public float pickupRadius = 5f;
    [SerializeField]
    private float itemFloatSpeed = 3f;
    void Update()
    {

        Collider[] detectedColliders = Physics.OverlapSphere(transform.position, pickupRadius);

        foreach (Collider collider in detectedColliders)
        {
            LootPickup loot = collider.gameObject.GetComponent<LootPickup>();
            if(loot == null)
                continue;
            if (loot.Available())
            {
                collider.transform.position = Vector3.Lerp(collider.transform.position, transform.position, Random.Range(itemFloatSpeed, itemFloatSpeed * 2) * Time.deltaTime);

            }


        }

    }
}

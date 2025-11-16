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

    [Header("Drop Settings")]
    [SerializeField] private OreData oreMined;
    [SerializeField] private ItemData itemDropped;
    [SerializeField] private GameObject lootPrefab;

    void Start()
    {
        ActivateDrill();
    }

    void ActivateDrill()
    {
        if(PowerManager.instance.PowerAvailability(powerUse))
        {
            hasPower = true;
            
            if(startAnimation == null)
            {
                startAnimation = StartCoroutine(DrillAnimation());
            }
        }
        else
        {
            hasPower = false;
            //Stop drill animation
        }
    }


    private IEnumerator DrillAnimation()
    {
        //Animations for drill rotation


        while(true)
        {

        }

        yield break;
    }


}

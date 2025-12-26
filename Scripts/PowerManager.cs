using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    /* Script to manage power levels and distribution in the game.
     * This script will handle power generation, consumption, and distribution to various systems.
     * It will also manage power outages and restoration.
     */
    #region MyRegionName Singleton Pattern


    public static PowerManager instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    private float maxPower = 100f;
    private float currentUnitsPerMin = 1000000f; //Testing

    [Header("GameObjects")]
    Dictionary<string, List<GameObject>> poweredObjects = new Dictionary<string, List<GameObject>>();

    
    private void OutOfPower()
    {
        //Effects
    }

    public void AddPowerGeneration(float rate)
    {
        maxPower += rate;
    }

    public void RemovePowerGeneration(float rate)
    {
        maxPower -= rate;
        if (maxPower < 0f) maxPower = 0f;
    }

    public void UsePower(float rate, GameObject obj)
    {
        currentUnitsPerMin += rate;
        
        //Add object
        string key = obj.name;
        if (!poweredObjects.TryGetValue(key, out List<GameObject> selected))
        {
            //If object type doesnt exist add to stack
            selected = new List<GameObject>();
            poweredObjects.Add(key, selected);
        }

        //Add power outage effects if out
        if(currentUnitsPerMin > maxPower)
        {
            OutOfPower();
        }
    }

    public void StopUsingPower(float rate, GameObject obj)
    {
        string key = obj.name;
        if (!poweredObjects.TryGetValue(key, out List<GameObject> selected))
        {
            //If object type doesnt exist add to stack
            selected = poweredObjects[key];
            selected.Remove(obj);
        }
        
        currentUnitsPerMin -= rate;
    }

    public bool CheckZonePower(float size)
    {
        //Check if current power generation meets the zone requirements
        //simple formula for testing purposes
        float powerDraw = size;
        return maxPower > (powerDraw + currentUnitsPerMin);

        //If not, trigger power outage effects
    }

    public bool PowerAvailability(float power)
    {
        return maxPower > power + currentUnitsPerMin;
    }

}

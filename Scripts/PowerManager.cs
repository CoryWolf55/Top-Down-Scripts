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
    

    public void AddPowerGeneration(float rate)
    {
        if(PowerAvailability(rate))
            maxPower += rate;
    }

    public void RemovePowerGeneration(float rate)
    {
        maxPower -= rate;
        if (maxPower < 0f) maxPower = 0f;
    }

    public void UsePower(float rate)
    {
        currentUnitsPerMin += rate;
    }

    public void StopUsingPower(float rate)
    {
        currentUnitsPerMin -= rate;
    }

    public bool PowerAvailability(float requiredUnitsPerMin)
    {
        return maxPower < currentUnitsPerMin + requiredUnitsPerMin;
    }

    public bool CheckZonePower(float size)
    {
        //Check if current power generation meets the zone requirements
        //simple formula for testing purposes
        float powerDraw = size;


        return false;

        //If not, trigger power outage effects
    }

}

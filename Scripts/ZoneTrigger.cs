using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    //Script for when player enters or leaves the zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ZoneManager.instance.InZone(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ZoneManager.instance.InZone(true);
        }
    }

}

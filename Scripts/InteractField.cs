using UnityEngine;

public class InteractField : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered interact field of " + gameObject.name);

            //Display UI
            //Test 
            ZoneManager.instance.ChangeZone(0f);
            
        }
    }
}

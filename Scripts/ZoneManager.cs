using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager instance { get; private set; }
    [SerializeField]
    private float speed = 2f;
    [SerializeField]
    private float startSize = 50f;
    public List<GameObject> generators = new List<GameObject>();
    private Vector3 newSize = Vector3.zero;
    private bool enableChange = false;
    [SerializeField]
    private float tolerance = 0.01f;

    private Coroutine zoneRoutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GetGenerators();
        ChangeZone(startSize);
    }





    public void ChangeZone(float size)
    {
        Debug.Log("Changing Zone to size: " + size);
        newSize = new Vector3(size, size, size);

        if (zoneRoutine != null)
            StopCoroutine(zoneRoutine);

         zoneRoutine = StartCoroutine(ChangeZoneRoutine());
    }

    private IEnumerator ChangeZoneRoutine()
    {
        while (Mathf.Abs(transform.localScale.x - newSize.x) > tolerance)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, newSize, Time.deltaTime * speed);
            Debug.Log(Mathf.Round(transform.localScale.x) + " : " + Mathf.Round(newSize.x));
            yield return null; // wait one frame
        }

        transform.localScale = newSize; // ensure exact final size
        Debug.Log("Finished changing zone.");
    }

    private void GetGenerators()
    {   
        GameObject[] g = GameObject.FindGameObjectsWithTag("Generator");

        //label generators
        float distance = 0;
        float lastDistance = 0;
        for(int i = 0; i < g.Length; i++)
        {
            distance = Vector3.Distance(this.transform.position, g[i].transform.position);

            if (lastDistance == 0)
            {
                generators.Add(g[i]);
                lastDistance = distance;
                continue;
            }
            if(distance < lastDistance)
            {
                generators.Insert(0, g[i]);
            }
            else
            {
                generators.Add(g[i]);
            }

            lastDistance = distance;
        }

    }

   
}

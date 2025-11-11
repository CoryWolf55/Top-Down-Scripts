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
        newSize = new Vector3(size, size, size);
        while(Mathf.Round(transform.localScale.x) != Mathf.Round(newSize.x))
        {
            transform.localScale = Vector3.Lerp(transform.localScale, newSize, Time.deltaTime * speed);    
        }


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

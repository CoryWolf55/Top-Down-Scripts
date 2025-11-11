/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraController : MonoBehaviour
{

    public float speed;
    public float sSpeed;
    public Transform target;
    public Vector3 offSet;
    private Vector3 defaultOffSet;
    public float maxOffSet;
    public float minOffSet;

    private void Start()
    {
        defaultOffSet = offSet;
    }



    void Update()
    {
        Adjustment();
        CameraMovement();
    }

    void CameraMovement()
    {
        
        transform.position = Vector3.Lerp(transform.position, target.transform.position + offSet, speed * Time.deltaTime);
 
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), sSpeed * Time.deltaTime);

    }

    private void Adjustment()
    {
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float scrollWheel = -Input.GetAxis("Mouse ScrollWheel") * 10;
           if(offSet.y < maxOffSet && offSet.y > minOffSet || offSet.y >= maxOffSet && scrollWheel < 0 || offSet.y <= minOffSet && scrollWheel > 0)
            {
                offSet.y += scrollWheel;
            }
           



        }
    }
}
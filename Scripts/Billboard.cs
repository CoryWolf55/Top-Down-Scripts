using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;
    private void Start()
    {
        if (cam == null)
        {
            cam = CameraController.instance.transform;
        }
    }
   
    void LateUpdate()
    {
        transform.LookAt(transform.position +cam.forward);
    }
}

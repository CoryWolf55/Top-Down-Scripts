/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot instance;

    private Gun currentPrimary;
    private Gun currentSecondary;
    private Gun currentGun;
    private Transform currentSlot;
    private Transform primarySlot;
    private Transform secondarySlot;
    private Transform ogTransform;
    private bool canShoot = true;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

        primarySlot = this.transform.GetChild(0);
        secondarySlot = this.transform.GetChild(1);
        currentPrimary = FindCurrentGun(true);
        currentSecondary = FindCurrentGun(false);
        currentGun = currentPrimary;
        ogTransform = gameObject.transform;
    }

    private void Update()
    {
        if(Input.GetMouseButton(0) && canShoot)
        {
            currentGun.Shoot();
        }

        if(Input.GetKey(KeyCode.Alpha1))
        {
            primarySlot.gameObject.SetActive(true);
            secondarySlot.gameObject.SetActive(false);
            currentGun = currentPrimary;
        }
        else if(Input.GetKey(KeyCode.Alpha2))
        {
            secondarySlot.gameObject.SetActive(true);
            primarySlot.gameObject.SetActive(false);
            currentGun = currentSecondary;
        }
    }




    private Gun FindCurrentGun(bool primary)
    {
        if(primary)
            currentSlot = primarySlot;
        else
            currentSlot = secondarySlot;

            for(int i = 0; i < currentSlot.childCount; i++)
            {
                if (currentSlot.GetChild(i).gameObject.activeSelf)
                {
                    return currentSlot.GetChild(i).GetComponent<Gun>();
                }
            }
            return null;
        
    }


    public void CanShoot(bool able)
    {
        canShoot = able;
    }
}
/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Gun : MonoBehaviour
{
    [Header("References")]
    public GunData gunData;
     private Transform muzzle;
    private float currentReloadTime;
    private bool canShoot = true;
    private float currentFireTime;
    [HideInInspector]
    public float spread;

    private void Awake()
    {
        muzzle = this.transform.Find("Muzzle");
        currentFireTime = gunData.fireTime;
        currentReloadTime = gunData.reloadTime;
        spread = gunData.spread;
    }

    public void Shoot()
    {
        if(!canShoot) { return; }
        if(gunData.reloading) { return; }

        canShoot = false;
        if(gunData.currentAmmo <= 0)
        {
            Reload();
            return;
        }
        gunData.currentAmmo--;

       // GameObject lastBullet = Instantiate(gunData.bullet.model, muzzle.position, muzzle.rotation);
        GameObject lastBullet = ObjectPoolManager.instance.Spawn(gunData.bullet.model, muzzle.position, muzzle.rotation);
        Rigidbody newBulletRigidbody = lastBullet.GetComponent<Rigidbody>();
        if (newBulletRigidbody != null || gunData.bullet.followRadius == 0)
        {
            Vector3 spr = new Vector3(Random.Range(spread,-spread) / 15f, 0f, Random.Range(spread,-spread) / 15f);
           // Vector3 p = new Vector3(PlayerController.instance.point.x,0f,PlayerController.instance.point.z);
           // Vector3 mz = new Vector3(muzzle.position.z,0,muzzle.position.z);
            newBulletRigidbody.linearVelocity = ((PlayerController.instance.point + spr) - muzzle.position).normalized * gunData.bulletSpeed;
        }
        else if(gunData.bullet.followRadius != 0)
        {

            lastBullet.transform.Rotate(new Vector3(0, Random.Range(spread, -spread) /10f, 0));
        }


        lastBullet.GetComponent<BulletController>().Initialize(gunData);
        
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
       ShootTimer();
       ReloadTimer();

    }

    private void ShootTimer()
    {
        if (canShoot) { return; }
        currentFireTime -= Time.deltaTime;
        if (currentFireTime <= 0)
        {
            currentFireTime = gunData.fireTime;
            canShoot = true;
        }
    }

    private void ReloadTimer() 
    {
        if(!gunData.reloading) { return;}
        currentReloadTime -= Time.deltaTime;
        if(currentReloadTime <= 0)
        {
            if(!gunData.infBullets)
            {
                if (gunData.currentReserve > 0)
                {
                    if (gunData.currentReserve > gunData.magSize)
                    {
                        gunData.currentReserve -= (gunData.magSize - gunData.currentAmmo);
                        gunData.currentAmmo = gunData.magSize;
                    }
                    else
                    {
                        gunData.currentAmmo = gunData.currentReserve;
                        gunData.currentReserve = 0;
                    }
                }
                else
                    Debug.Log("Out of Ammo");
            }
            else
                gunData.currentAmmo = gunData.magSize;
            
            currentReloadTime = gunData.reloadTime;
            gunData.reloading = false;

        }
    }

    private void Reload()
    {
        gunData.reloading = true;
    }
   
}
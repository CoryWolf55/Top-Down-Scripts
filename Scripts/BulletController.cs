/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SocialPlatforms;

public class BulletController : MonoBehaviour
{
    public string enemyTag = "Enemy";
    private float lifeTime;
    private float speed;
    private GunData gunData;
    private GameObject hitTarget;
    private List<GameObject> enemiesInRange = new List<GameObject>();
    private Transform target;

    private void Start()
    {
        //if(gunData.bullet.followRadius != 0)
           // InvokeRepeating("TrackingBullet", 0f, 0.2f);
    }

    private void Update()
    {
       // Rotation();
       TrackingBullet();
        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0) Destroy(gameObject);


    }
    #region tracking
    private void TrackingBullet()
    {
        if (gunData.bullet.followRadius == 0) { return; }
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        Collider[] detectedColliders = Physics.OverlapSphere(transform.position, gunData.bullet.followRadius);
       
        foreach(Collider collider in detectedColliders)
        {
            if(collider.gameObject.CompareTag(enemyTag))
            {
                enemiesInRange.Add(collider.gameObject);
            }
         /* else if(enemiesInRange.Contains(collider.gameObject))
            {
               enemiesInRange.Remove(collider.gameObject);
            }*/
         
        }


        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemiesInRange)
        {
            if(enemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }
            
        }

        if (nearestEnemy != null && shortestDistance <= gunData.bullet.followRadius)
        {
            target = nearestEnemy.transform;
            Vector3 dir = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, gunData.bullet.turnSpeed * Time.deltaTime).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }
        else
        {
            target = null;
        }
        

    }

 /*   private void Rotation()
    {
        if(gunData.bullet.followRadius == 0) { return;}

        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, gunData.bullet.turnSpeed * Time.deltaTime).eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

    }*/
    #endregion tracking

    public void Initialize(GunData gunDat)
    {
        speed = gunDat.bulletSpeed;
        lifeTime = gunDat.bullet.duration;
        gunData = gunDat;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (gunData.bullet.expRadius != 0)
        {
            ParticleSystem pc = Instantiate(gunData.bullet.particleSystem, transform.position, Quaternion.identity);
            Destroy(pc, 3);
            Explosion();
        }
        if (collision.gameObject.CompareTag(enemyTag))
        { 
            hitTarget = collision.gameObject;
            if(gunData.bullet.effect != null)
            {
                EffectController(); 
            }
            
           if(hitTarget.GetComponent<EnemyController>() != null)
            {
                hitTarget.GetComponent<EnemyController>().TakeDamage(gunData.damage);
            }
        }
        
            Destroy(gameObject);
       

    }

    private void Explosion()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, gunData.bullet.expRadius);

        foreach (Collider nearby in colliders)
        {
            
            //damage enemies here

            //Raycast to nearest enemies(within radius), if there is an obstical in the way ignore damage, divide damage by distance if less than 1 = max damage
            
            //pushes them back
            /*
            Rigidbody rigg = nearby.GetComponent<Rigidbody>();
            if (rigg != null)
            {
                rigg.AddExplosionForce(gunData.bullet.expForce, transform.position, gunData.bullet.expRadius);
            }
            */
        }
    }

    private void EffectController()
    {
        if (hitTarget.GetComponent<Effect>() == null ||
                    hitTarget.GetComponent<Effect>().currentEffect != gunData.bullet.effect)
        {
            hitTarget.AddComponent<Effect>();
            hitTarget.GetComponent<Effect>().EffectInit(gunData.bullet.effect);
        }
        else
        {
            hitTarget.GetComponent<Effect>().EffectInit(gunData.bullet.effect);
        }
    }
}
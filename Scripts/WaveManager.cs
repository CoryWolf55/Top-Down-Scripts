/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
public class WaveManager : MonoBehaviour
{
    public Enemy[] enemyList;
   // private List<Enemy> queue = new List<Enemy>();
    [Header("Timers")]
    [SerializeField] private float waveBreakTime;
    private float currentWaveBreakTime;
    private bool startBreakTime = true;
    [SerializeField] private float spawnBufferTime;
    private float currentSpawnBufferTime;
    [SerializeField] private float waveDuration;
    private float currentWaveDuration;
    [SerializeField] private int currentWave;
    [Header("Points")]
    [SerializeField] private int startingWavePoints;
    [SerializeField] private int wavePointMultiplier;
    private int currentWavePoints;
    private int currentMaxPoints;
    [Header("Attributes")]
    [SerializeField] private bool waveStarted;
    private bool spawning = true;


    [Header("Defence Waves")]
    [SerializeField] private bool defenseWave;
    [SerializeField] private int startingDefenseWavePoints;
    [SerializeField] private int defenseWavePointMultiplier;
    private int currentDefenseWavePoints;
    private int currentDefenseMaxPoints;
    private int currentDefenseWave;

    private void Start()
    {
        currentWaveBreakTime = waveBreakTime;
        currentSpawnBufferTime = spawnBufferTime;
        currentWaveDuration = waveDuration;
        currentWavePoints = startingWavePoints;
        currentMaxPoints = startingWavePoints;

        //Defense wave
        currentDefenseWavePoints = startingDefenseWavePoints;
        currentDefenseWave = 0;
        currentDefenseMaxPoints = startingDefenseWavePoints;
    }


    private void Update()
    {
        if(startBreakTime)
        {
            BreakTimer();  
        }
        else
        {
            WaveTimer();
        }
        if(!startBreakTime && Spawner.instance.currentEntities.Count <= 0 && !spawning)
        {
            startBreakTime = true;
        }

        if(defenseWave)
        {
            SpawnDefenseWave();
            return;
        }

        if(waveStarted && spawning)
        {
            SpawnWave();
        }

    }

    private void BreakTimer()
    {
        currentWaveBreakTime -= Time.deltaTime;
        if (currentWaveBreakTime <= 0)
        {
            waveStarted = true;
            startBreakTime = false;
            spawning = true;
            currentWaveBreakTime = waveBreakTime;
        }
    }

    private void WaveTimer()
    {
        currentWaveDuration -= Time.deltaTime;
        if (currentWaveDuration <= 0)
        {
            waveStarted = false;
            currentWaveDuration = waveDuration;
        }
    }

    private void SpawnWave()
    {
        while (currentWavePoints > 0)
        {
            currentSpawnBufferTime -= Time.deltaTime;
            if (currentSpawnBufferTime <= 0)
            {
                currentSpawnBufferTime = spawnBufferTime;
                spawning = true;
                Enemy currentEnemy = enemyList[UnityEngine.Random.Range(0, enemyList.Length)];
                if (currentEnemy.waveCost <= currentWavePoints)
                {
                    currentWavePoints -= currentEnemy.waveCost;
                    //Reseting the speed then spawning
                    // currentEnemy.enemyData.speed = currentEnemy.enemyData.defaultSpeed;
                    Spawner.instance.SpawnEntity(currentEnemy.enemyData.prefab, Spawner.instance.FindRandomLocation(), 1);
                    Debug.Log(currentEnemy);
                }
            }
        }
        if (currentWavePoints <= 0)
        {
            spawning = false;
            currentMaxPoints *= wavePointMultiplier;
            currentWavePoints = currentMaxPoints;
            currentWave++;
            Debug.Log("EndSpawning");
        }
    }



    private void SpawnDefenseWave()
    {
        while (currentDefenseWavePoints > 0)
        {
            currentSpawnBufferTime -= Time.deltaTime;
            if (currentSpawnBufferTime <= 0)
            {
                currentSpawnBufferTime = spawnBufferTime;
                spawning = true;
                Enemy currentEnemy = enemyList[UnityEngine.Random.Range(0, enemyList.Length)];
                if (currentEnemy.waveCost <= currentWavePoints)
                {
                    currentWavePoints -= currentEnemy.waveCost;
                    //Reseting the speed then spawning
                    // currentEnemy.enemyData.speed = currentEnemy.enemyData.defaultSpeed;
                    Spawner.instance.SpawnEntity(currentEnemy.enemyData.prefab, Spawner.instance.FindRandomLocation(), 1);
                    Debug.Log(currentEnemy);
                }
            }
        }

        spawning = false;
        currentDefenseMaxPoints *= defenseWavePointMultiplier;
        currentDefenseWavePoints = currentDefenseMaxPoints;
        currentDefenseWave++;
        Debug.Log("EndSpawning");
    }

}



[System.Serializable]
public class Enemy
{
    public EnemyData enemyData;
    public int waveCost;
}
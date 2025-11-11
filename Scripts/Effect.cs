/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Effect : MonoBehaviour
{
    public EffectData currentEffect;
    private float currentDuration;
    private float maxDuration;
    private float currentTick;
    private float maxTick;
    
    void Update()
    {
        currentDuration -= Time.deltaTime;
        if(currentDuration <= 0)
        {
            Destroy(this);
        }

        currentTick -= Time.deltaTime;
        if(currentTick <= 0)
        {
            Apply();
            currentTick = maxTick;
        }
    }

    public void EffectInit(EffectData effect)
    {
        currentEffect = effect;
        maxDuration = effect.duration;
        currentDuration = maxDuration;
        maxTick = effect.tickSpeed;
    }

    private void Apply()
    {
        Debug.Log(currentEffect);
    }
}
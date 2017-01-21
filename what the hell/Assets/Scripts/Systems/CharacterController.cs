﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    Animator animatorControl;
    [SerializeField]
    string  startChargingTrigger;
    [SerializeField]
    string unleashTrigger;
    [SerializeField]
    string jumpCompletionValueName;
    [SerializeField]
    string horizontalSpeedValueName;

    [SerializeField]
    timerCurve jumpHeightCurve;
    [SerializeField]
    AnimationCurve accelerationCurve;
    [SerializeField]
    AnimationCurve decelerationCurve;

    float startingHeight;
    [SerializeField]
    float movementSpeed;
    float currentSpeed;
    float lastAccelerationTime;
    [SerializeField]
    float jumpMaxHeight;
    bool isDecelerating;
    float startAccumulatoinJumpPowerTime;
    [SerializeField]
    float maxAccumulatoinJumpPowerTime;
    [SerializeField]
    [Range(0.1f,1)]
    float minJumpPower;

    bool chargingWasSet = false;

    public void Start()
    {
        jumpHeightCurve.setOver();
        startingHeight = transform.position.y;
    }

    public void StartAccumulatingJumpPower() {
        //Debug.Log("StartAccumulatingJumpPower");
        animatorControl.SetTrigger(startChargingTrigger);
        chargingWasSet = true;
        startAccumulatoinJumpPowerTime = Time.time;
    }
    public bool KeepAccumulatingJumpPower()
    {
        if(!chargingWasSet)
        {
            animatorControl.SetTrigger(startChargingTrigger);
            chargingWasSet = true;
        }
        //Debug.Log("KeepAccumulatingJumpPower");
        return Time.time - startAccumulatoinJumpPowerTime < maxAccumulatoinJumpPowerTime;
    }
    public bool IsJumping()
    {
        return !jumpHeightCurve.isOver;
    }
    public void Jump()
    {
        if (jumpHeightCurve.isOver)
        {
            //Debug.Log("Jump");
            animatorControl.SetTrigger(unleashTrigger);
            float accumulation = Mathf.Clamp( Time.time - startAccumulatoinJumpPowerTime, 0, maxAccumulatoinJumpPowerTime);
            float jumpPowerCoefficient = Mathf.Clamp( accumulation/ maxAccumulatoinJumpPowerTime , minJumpPower,1);
            jumpHeightCurve.Reset();
            UniqueCoroutine.doUntil(this, delegate ()
            {
                jumpHeightCurve.refreshTime();
                transform.position = new Vector3(transform.position.x, jumpHeightCurve.CurrentValue*jumpMaxHeight*jumpPowerCoefficient, transform.position.z);
                startAccumulatoinJumpPowerTime = Time.time;
                animatorControl.SetFloat(jumpCompletionValueName, jumpHeightCurve.CurrentPercent);
                if(jumpHeightCurve.CurrentPercent>=.9f)
                    chargingWasSet = false;
            }, delegate () { return !jumpHeightCurve.isOver; });
        }
    }


    public void Move(float xAxisSpeed) {
        currentSpeed = Mathf.Clamp(currentSpeed+ xAxisSpeed *Time.deltaTime,-1,1);
        //if current speed sign not equal to axis speed sign their product is less then 0 and therefore is decelerating
        isDecelerating = currentSpeed * xAxisSpeed < 0|| xAxisSpeed==0;
        if(!isDecelerating)
        { lastAccelerationTime = Time.time; }
    }

    void updateXposition() {
        animatorControl.SetFloat(horizontalSpeedValueName, currentSpeed);
        //update pos
        transform.position += Vector3.right * accelerationCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(currentSpeed))) * movementSpeed * (currentSpeed > 0 ? 1 : -1);

        //dampen current speed
        if (isDecelerating)
            currentSpeed = decelerationCurve.Evaluate(Time.time - lastAccelerationTime) *(currentSpeed>0?1:-1);
    }
    public void Struggle() { }

    void Update() { 
        updateXposition();
    }

}

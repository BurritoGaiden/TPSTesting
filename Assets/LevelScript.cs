﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour {

    //Declaring the delegate
    public delegate void ObjectiveDelegate(string objName, string objDesc, int objType, string objTargetNameBase);
    public static event ObjectiveDelegate thisObjective;

    public delegate void DialogueDelegate(int chosenDialogueLine);
    public static event DialogueDelegate ThisDialogue;

    public delegate void CharDisableDelegate();
    public static event CharDisableDelegate DCharInput;

    public delegate void CharEnableDelegate();
    public static event CharEnableDelegate ECharInput;

    public delegate void CamDisableDelegate();
    public static event CamDisableDelegate DCamInput;

    public delegate void CamEnableDelegate();
    public static event CamEnableDelegate ECamInput;

    public delegate void CameraDelegate();
    public static event CameraDelegate ResetCamPositionOnRig;

    public delegate void CameraTransformDelegate(Vector3 pos,Vector3 rot);
    public static event CameraTransformDelegate SetCharCamTransform;

    public delegate void InterestDelegate(String thisOne);
    public static event InterestDelegate enableInterestTrigger;
    public static event InterestDelegate disableInterestTrigger;

    //Use this to pause and resume the level script
    public bool runScript;
    public static bool coroutinePause = true;
    public static bool waitTillObjectiveDone;

    //Testing public vars
    public Vector3[] tempCamPos;
    public Vector3[] tempCamRot;

    // Use this for initialization
    void Awake() {
        ObjectiveHandler.ObjDone += ObjectiveDoneListener;
    }
       
	void Start () {
        if(runScript == true)
        StartCoroutine(MainLevelCoroutine());
	}

    void ObjectiveDoneListener() {
        waitTillObjectiveDone = false;
    }

    //Script for the level
    IEnumerator MainLevelCoroutine()
    {
        DCharInput();
        SetCharCamTransform(tempCamPos[0], tempCamRot[0]);
        ThisDialogue(0);
        yield return new WaitForSeconds(2);
        ThisDialogue(1);
        yield return new WaitForSeconds(2);
        SetCharCamTransform(tempCamPos[1], tempCamRot[1]);
        yield return new WaitForSeconds(2);
        //print(thisObjective("Walking Time", "Walk to the white spot", 3, "obj3Targ"));
        thisObjective("Walking Time", "Walk to the white spot", 3, "obj3Targ");
        print(ECharInput);
        ECharInput();
        print(ECamInput);
        ECamInput();
        print(ResetCamPositionOnRig);
        ResetCamPositionOnRig();
        print(enableInterestTrigger);
        enableInterestTrigger("Int1");

        //Wait till the player has finished this objective
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        //disableInterestTrigger("Int1");

        ThisDialogue(2);
        DCamInput();
        DCharInput();
        SetCharCamTransform(tempCamPos[2], tempCamRot[2]);
        yield return new WaitForSeconds(4);
        ResetCamPositionOnRig();
        ECamInput();
        ECharInput();
        thisObjective("Collecting Time", "Collect 3 white greyboxes", 1, "obj1Targ");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        ThisDialogue(3);
        yield return new WaitForSeconds(5);

        thisObjective("Killing Time", "Kill the bot", 2, "obj2Targ");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        print("ayy made it");
        yield return new WaitForSeconds(3f);

        ThisDialogue(4);
        print("Level Complete");
    }

    //TODO: program a delegate that allows calls for specific targets on an objective being completed
}

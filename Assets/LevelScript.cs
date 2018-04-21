using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour {

    //Declaring the delegate
    public delegate void ObjectiveDelegate(string objName, string objDesc, int objType, string objTargetNameBase);
    public static event ObjectiveDelegate thisObjective;

    public delegate void DialogueDelegate(int chosenDialogueLine);
    public static event DialogueDelegate ThisDialogue;

    public delegate void InputDelegate();
    public static event InputDelegate disableCharInput;
    public static event InputDelegate enableCharInput;
    public static event InputDelegate disableCamInput;
    public static event InputDelegate enableCamInput;

    public delegate void CameraDelegate();
    public static event CameraDelegate ResetCamPositionOnRig;

    public delegate void CameraTransformDelegate(Vector3 pos,Vector3 rot);
    public static event CameraTransformDelegate SetCharCamTransform;

    //Use this to pause and resume the level script
    public bool runScript;
    public static bool coroutinePause = true;
    public static bool waitTillObjectiveDone;
    public bool catchBool;

    //Testing public vars
    public Vector3 tempCamPos;
    public Vector3 tempCamRot;

	// Use this for initialization
	void Start () {
        ObjectiveHandler.ObjDone += ObjectiveDoneListener;
        ObjectiveHandler.OneTargetAchieved += TargetAchievedCallback;
        if(runScript == true)
        StartCoroutine(MainLevelCoroutine());
	}

    void ObjectiveDoneListener() {
        waitTillObjectiveDone = false;
    }

    //Script for the level
    IEnumerator MainLevelCoroutine()
    {
        catchBool = false;
        disableCharInput();
        ThisDialogue(0);
        yield return new WaitForSeconds(2);
        ThisDialogue(1);
        yield return new WaitForSeconds(4);
        thisObjective("Walking Time", "Walk to the white spot", 3, "obj3Targ");
        enableCharInput();
        enableCamInput();
        ResetCamPositionOnRig();

        //Wait till the player has finished this objective
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) {
            
            yield return null;

        }

        ThisDialogue(2);
        disableCamInput();
        disableCharInput();
        SetCharCamTransform(tempCamPos, tempCamRot);
        yield return new WaitForSeconds(4);
        ResetCamPositionOnRig();
        enableCamInput();
        enableCharInput();
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
    void TargetAchievedCallback() {
        //if(theCatch == )
        catchBool = true;
    }
}

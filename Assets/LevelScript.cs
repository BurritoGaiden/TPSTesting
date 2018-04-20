using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour {

    //Declaring the delegate
    public delegate void ObjectiveDelegate(string objName, string objDesc, int objType, string objTargetNameBase);
    public static event ObjectiveDelegate thisObjective;

    public delegate void DialogueDelegate(int chosenDialogueLine);
    public static event DialogueDelegate ThisDialogue;

    //Use this to pause and resume the level script
    public static bool coroutinePause = true;
    public static bool waitTillObjectiveDone;

	// Use this for initialization
	void Start () {
        ObjectiveHandler.ObjDone += ObjectiveDoneListener;
        StartCoroutine(MainLevelCoroutine());
	}

    void ObjectiveDoneListener() {
        waitTillObjectiveDone = false;
    }

    //Script for the level
    IEnumerator MainLevelCoroutine()
    {
        ThisDialogue(0);
        yield return new WaitForSeconds(2);
        ThisDialogue(1);
        yield return new WaitForSeconds(4);
        thisObjective("Walking Time", "Walk to the white spot", 3, "obj3Targ");
        
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        ThisDialogue(2);
        yield return new WaitForSeconds(4);

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
}

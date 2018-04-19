using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour {

    //Declaring the delegate
    public delegate void ObjectiveDelegate(string objName, string objDesc, int objType, string objTargetNameBase);
    public static event ObjectiveDelegate thisObjective;

    //Use this to pause and resume the level script
    public bool coroutinePause = true;

	// Use this for initialization
	void Start () {
        StartCoroutine(MainLevelCoroutine());
	}

    //Script for the level
    IEnumerator MainLevelCoroutine()
    {
        thisObjective("Collecting Time", "Collect 3 white greyboxes", 1, "obj1Targ");

        while (coroutinePause == true)
        {
            yield return null;
        }

        

        //print("Reached the target.");
        
        yield return new WaitForSeconds(3f);
        
        print("Level Complete");
    }
}

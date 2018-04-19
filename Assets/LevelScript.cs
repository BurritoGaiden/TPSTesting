using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour {

    //Use this to pause and resume the level script
    public bool coroutinePause = true;

	// Use this for initialization
	void Start () {
        StartCoroutine(MainLevelCoroutine());
	}

    //Script for the level
    IEnumerator MainLevelCoroutine()
    {
        while(coroutinePause == true)
        {
            yield return null;
        }
        
        print("Reached the target.");
        
        yield return new WaitForSeconds(3f);
        
        print("Level Complete");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interestable : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        LevelScript.EnableInterestTrigger += ActivateTrigger;
        LevelScript.DisableInterestTrigger += DeactivateTrigger;
        this.GetComponent<BoxCollider>().enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void ActivateTrigger(string thisOne) {
        if(thisOne == this.transform.name)
        this.GetComponent<BoxCollider>().enabled = true;
    }

    void DeactivateTrigger(string thisOne)
    {
        if (thisOne == this.transform.name)
            this.GetComponent<BoxCollider>().enabled = false;
    }
}

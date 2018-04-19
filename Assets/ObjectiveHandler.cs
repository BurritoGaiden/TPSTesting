using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveHandler : MonoBehaviour {

    ///Rules
    ///create a mutable list of objectives X
    ///create a function that adds an objective to the stack/list X
    ///create a function that removes an objective from the stack/list
    ///create a few different kinds of objectives
    ///collect
    ///kill
    ///go to
    ///player perform action/player character be in state
    ///create markers that can indicate objectives for the player

    //Current Data
    private float currentTimeTillTextOff;
    List<Objective> objectivesList = new List<Objective>();

    //Interface
    public Text objectiveTextBox;
    public AudioSource objectiveAudio;

	// Use this for initialization
	void Start () {
        objectivesList.Add(new Objective("Collecting Time", "Collect 3 white greyboxes", 1, "obj1Targ"));
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Y)) {
            //objectivesList.Add(new Objective("Collecting Time", "Collect 3 white greyboxes", 1));
        }

        PresentObjective();
    }

    void PresentObjective() {
        objectiveTextBox.text = objectivesList[0].name + ": " + objectivesList[0].description;
        //Check if only one target exists
        if (objectivesList[0].targetList.Capacity == 1) {
            objectiveTextBox.text += "\\n " + objectivesList[0].targetList[0].name;
            return;
        }
        //Check if subsequent targets exist
        for (int i = 0; i < objectivesList[0].targetList.Capacity; i++) {
            objectiveTextBox.text += "\\n " + objectivesList[0].targetList[i].name;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveHandler : MonoBehaviour {

    ///Rules
    ///create a mutable list of objectives X
    ///create a function that adds an objective to the stack/list 
    ///create a function that removes an objective from the stack/list 
    ///create a few different kinds of objectives
    ///collect
    ///kill
    ///go to
    ///player perform action/player character be in state
    ///create markers that can indicate objectives for the player X 

    //Reminder: Don't check if a list entry is null, because it doesn't exist. Check the count.

    //Current Data
    private float currentTimeTillTextOff;
    List<Objective> objectivesList = new List<Objective>();

    //Resources
    public GameObject objectiveTrackerPrefab;
    public GameObject worldSpaceCanvas;

    //Interface
    public Text objectiveTextBox;
    public AudioSource objectiveAudio;
    List<Image> objectiveTrackers = new List<Image>();

    //Declaring the delegate
    public delegate void TargetNameBaseDelegate(string tBase);
    public static event TargetNameBaseDelegate targetName;

    public delegate void ObjectiveDelegate();
    public static event ObjectiveDelegate ObjDone;

    // Use this for initialization
    void Awake () {
        Collecting.collectThis += ObjectiveProgressUpdate;
        Walking.WalkThis += ObjectiveProgressUpdate;
        Killing.killThis += ObjectiveProgressUpdate;
        LevelScript.AssignThisObjective += AddObjective;
	}
	
	// Update is called once per frame
	void Update () {
        if(objectiveTextBox)
        PresentObjective();
        if(objectiveTextBox)
        VisuallyTrackObjectives();
    }

    void LateUpdate() {
        CheckIfObjectiveComplete();
    }

    void CheckIfObjectiveComplete() {
        if (objectivesList.Count != 0) //If there is an objective present
        {
            if (objectivesList[0].targetList.Count == 0) //If that objective's targets are all achieved
            {
                objectivesList.Remove(objectivesList[0]); //Remove the objective and tell the objective handler
                ObjDone();
            }
        }
    }

    #region Visual representation for objectives in world
    //Current objective text in the UI
    void PresentObjective() {
        if (objectivesList.Count != 0)
        {
            objectiveTextBox.text = objectivesList[0].name + ": " + objectivesList[0].description;

            //Check if targets exist for the objective, add them to objective UI
            for (int i = 0; i < objectivesList[0].targetList.Count; i++)
            {
                objectiveTextBox.text += "\n " + objectivesList[0].targetList[i].name;
            }
        }
        else
        {
            objectiveTextBox.text = "";
        }
    }

    //We always track the first objective "objective[0]", visually represent target locations
    void VisuallyTrackObjectives() {

        //If there's an objective, for each target, create and manage a tracker for it
        if (objectivesList.Count != 0)
        {
            //creating new tracker, adding to world space canvas
            if (objectiveTrackers.Count < objectivesList[0].targetList.Count)
            {
                float trackerDiff = objectivesList[0].targetList.Count - objectiveTrackers.Count;
                for (int i = 0; i < trackerDiff; i++)
                {
                    GameObject newTracker = Instantiate(objectiveTrackerPrefab);
                    newTracker.GetComponent<Image>().rectTransform.SetParent(worldSpaceCanvas.transform);

                    //dealing with weird UI scaling
                    newTracker.GetComponent<Image>().rectTransform.localScale = new Vector3(1, 1, 1);
                    newTracker.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(.4f, .4f);
                    objectiveTrackers.Add(newTracker.GetComponent<Image>());
                }
            }
            else if (objectiveTrackers.Count > objectivesList[0].targetList.Count)
            {
                float trackerDiff = objectiveTrackers.Count - objectivesList[0].targetList.Count;
                for (int i = 0; i < trackerDiff; i++)
                {
                    GameObject thatTracker = objectiveTrackers[i].gameObject;
                    objectiveTrackers.Remove(thatTracker.GetComponent<Image>());
                    Destroy(thatTracker);
                }
            }

            //managing each tracker position
            for (int i = 0; i < objectivesList[0].targetList.Count; i++)
            {
                float trackerX = objectivesList[0].targetList[i].transform.position.x;
                float trackerY = objectivesList[0].targetList[i].transform.position.y + 1;
                float trackerZ = objectivesList[0].targetList[i].transform.position.z;
                objectiveTrackers[i].rectTransform.position = new Vector3(trackerX, trackerY, trackerZ);
            }
        }
        else {
            for (int l = 0; l < objectiveTrackers.Count; l++)
            {
                GameObject thatTracker = objectiveTrackers[l].gameObject;
                objectiveTrackers.Remove(thatTracker.GetComponent<Image>());
                Destroy(thatTracker);
            }
        }
    }
    #endregion

    //Update progress on an objective
    void ObjectiveProgressUpdate(GameObject refGameObject) {
        if (objectivesList.Count != 0)
        {
            for (int i = 0; i < objectivesList[0].targetList.Count; i++)
            {
                if (objectivesList[0].targetList[i].name == refGameObject.name)
                {
                    objectivesList[0].targetList.Remove(objectivesList[0].targetList[i]);
                }
            }
        }
    }

    void AddObjective(string objName,string objDesc, int objType, string objTargetNameBase) {
        objectivesList.Add(new Objective(objName, objDesc, objType, objTargetNameBase));
        targetName(objTargetNameBase);
    }
}

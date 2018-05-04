using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    public GameObject player;
    public GameObject truck;
    public Transform[] truckPositions;

    public AudioMixer thisMixer;
    public AudioMixerSnapshot[] theseSnapshots;

    //This is more of a game manager thing
    public static GamePlayState thisGameplayState = GamePlayState.Regular;
    public Playmode thisPlayMode = Playmode.Linear;
    

    // Use this for initialization
    void Awake() {
        ObjectiveHandler.ObjDone += ObjectiveDoneListener;
    }
       
	void Start () {
        if (runScript == true)
            //StartCoroutine(MainLevelCoroutine());
            StartCoroutine(TruckLevelCoroutine());
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
        ECharInput();
        ECamInput();
        ResetCamPositionOnRig();
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

    //Script for the level
    IEnumerator TruckLevelCoroutine()
    {
        //Start level
        truck.SetActive(false);
        theseSnapshots[0].TransitionTo(3f);
        DCharInput();

        //Joel is waking up
        ThisDialogue(0);
        yield return new WaitForSeconds(2);
        ThisDialogue(1);
        yield return new WaitForSeconds(2);
        SetCharCamTransform(tempCamPos[1], tempCamRot[1]);
        yield return new WaitForSeconds(2);
        thisObjective("Walk Thing 1", "Walk to the white spot", 3, "truckTrig1");
        ECharInput();
        ECamInput();
        ResetCamPositionOnRig();
        enableInterestTrigger("Int1");

        //Wait till the player has finished this objective
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        theseSnapshots[1].TransitionTo(3f);
        truck.SetActive(true);
        truck.transform.position = truckPositions[0].position;

        thisObjective("Walk Thing 4", "Walk to the white spot", 3, "truckTrig4");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        truck.SetActive(false);
        theseSnapshots[0].TransitionTo(4f);



        thisObjective("Walk Thing 2", "Walk to the white spot", 3, "truckTrig2");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        truck.SetActive(true);
        theseSnapshots[1].TransitionTo(.5f);
        truck.transform.position = truckPositions[2].position;
        PlayerCamera.cameraState = camStates.STATE_DIRFOCUS;
        DCharInput();
        PlayerCamera.camTar = truck.transform;

        float counter = 0;
        while (counter < 4) {
            counter += Time.deltaTime;
            Debug.Log("Have waited" + counter + " seconds");
            RailPlayer();
            yield return null;
        }

        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        ECharInput();

        thisObjective("Walk Thing 3", "Walk to the white spot", 3, "truckTrig3");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone)
        {
            if (PlayerController.triggerCollidingWith) {
                if (PlayerController.triggerCollidingWith.tag == "Trigger") {
                    truck.transform.position = new Vector3(player.transform.position.x, truck.transform.position.y, truck.transform.position.z);
                }
            }

            yield return null;
        }
        truck.transform.position = truckPositions[1].position;
        theseSnapshots[2].TransitionTo(3f);

        print("ey");
        
    }

    //TRUCK MOVERS

    public Rail rail;

    private int currentSeg;
    private float transition;
    private bool isCompleted;

    void Update() {

    }
    void RailPlayer()
    {
        if (!rail)
            return;

        if (!isCompleted)
            PlayRail();
    }

    void PlayRail() {
        transition += Time.deltaTime * 1 / 1.2f;
        if (transition > 1)
        {
            transition = 0;
            currentSeg++;
        }
        else if (transition < 0) {
            transition = 1;
            currentSeg--;
        }

        truck.transform.position = rail.PositionOnRail(currentSeg, transition, thisPlayMode);
        truck.transform.rotation = rail.Orientation(currentSeg, transition);
    }

    //TODO: program a delegate that allows calls for specific targets on an objective being completed
}

public enum GamePlayState {
    Regular,
    Combat,
    Finale
}
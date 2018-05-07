using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

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

    public AudioClip[] sfx;

    public GameObject fallingPiece;


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

    void Update() {
        if (PlayerController.health <= 0) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
        theseSnapshots[0].TransitionTo(0f);
        DCharInput();

        //Joel is waking up
        ThisDialogue(0);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);
        ECharInput();
        ECamInput();
        ResetCamPositionOnRig();
        ThisDialogue(1);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        thisObjective("Find a way out", "", 3, "roomTrig1");
        print("before break");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        print("after break");
        DCamInput();
        DCharInput();
        SetCharCamTransform(tempCamPos[1], tempCamRot[1]);
        truck.SetActive(true);
        truck.transform.position = truckPositions[0].position;
        this.GetComponent<AudioSource>().PlayOneShot(sfx[0], 6);
        theseSnapshots[1].TransitionTo(.5f);
        yield return new WaitForSeconds(1f);
        ThisDialogue(2);
        yield return new WaitForSeconds(3f);

        ThisDialogue(3);        
        ECharInput();
        ECamInput();
        ResetCamPositionOnRig();

        thisObjective("Find the secret exit", "", 3, "truckTrig4");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //The player has navigated through the secret exit
        ThisDialogue(4);
        truck.SetActive(false);
        theseSnapshots[0].TransitionTo(1f);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        thisObjective("Continue", "", 3, "truckTrig2");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //The Player has hit the truck trigger
        fallingPiece.transform.localPosition = new Vector3(fallingPiece.transform.position.x, -.5f,fallingPiece.transform.position.z);
        truck.SetActive(true);
        theseSnapshots[1].TransitionTo(.5f);
        PlayerCamera.cameraState = camStates.STATE_DIRFOCUS;
        DCharInput();
        PlayerCamera.camTar = truck.transform;
        currentSeg = 1;
        float counter = 0;
        while (counter < 2.5) {
            counter += Time.deltaTime;
            RailPlayer();
            yield return null;
        }

        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        ECharInput();
        ThisDialogue(5);

        thisObjective("Run upstairs!", "", 3, "truckTrig9");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        //Player must collect the pieces
        thisObjective("Collect 3 boxes", "", 1, "truckTrig6");

        ThisDialogue(6);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        ThisDialogue(7);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);
       
        //Player sets the bomb
        thisObjective("Set the bomb by the broken window", "", 1, "truckTrig7");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        ThisDialogue(8);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        ThisDialogue(9);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        //Wait till the player falls down
        thisObjective("Fall down the hole", "", 3, "truckTrig5");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        //Tell the player they'll have to stay in cover
        ThisDialogue(10);
        GetComponent<AudioSource>().PlayOneShot(sfx[1], 3);
        DCharInput();
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);
        ECharInput();

        thisObjective("Move to the next piece of cover", "", 3, "chaseTrig1");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        ThisDialogue(11);
        //yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        thisObjective("Make it to the end of the hallway", "", 3, "truckTrig3");

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

        ThisDialogue(12);
        truck.transform.position = truckPositions[1].position;
        theseSnapshots[2].TransitionTo(3f);

        //when the player leaves
        thisObjective("Run", "Get to the end of the corridor", 3, "truckTrig8");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //ThisDialogue(16);

        print("ey");       
    }

    //TRUCK MOVERS

    public Rail rail;

    private int currentSeg;
    private float transition;
    private bool isCompleted;

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
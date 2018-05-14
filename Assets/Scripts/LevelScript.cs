using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

//The LevelScript is the "Director" of the level, determing all gameplay events in a linear fashion
public class LevelScript : MonoBehaviour {
    
//The LevelScript calls delegates that tell other objects in the scene to perform specific methods, like actors being told to do an action
//This approach works with treating everything in a modular fashion, and decouples objects from each other

    //Delegates for the objects consistent across all levels
    #region Objective, Player, and Camera Delegates
    public delegate void ObjectiveDelegate(string objName, string objDesc, int objType, string objTargetNameBase);
    public static event ObjectiveDelegate AssignThisObjective;

    public delegate void DialogueDelegate(int chosenDialogueLine);
    public static event DialogueDelegate PlayThisDialogue;

    public delegate void CharacterDelegate();
    public static event CharacterDelegate DisableCharacterInput, EnableCharacterInput;

    public delegate void CameraDelegate();
    public static event CameraDelegate DisableCameraInput, EnableCameraInput, ResetCamPositionOnRig;

    public delegate void CameraTransformDelegate(Vector3 pos,Vector3 rot);
    public static event CameraTransformDelegate SetCharCamTransform;

    public delegate void InterestDelegate(String thisOne);
    public static event InterestDelegate EnableInterestTrigger, DisableInterestTrigger;

    public delegate void TruckDelegate();
    public static event TruckDelegate DisableTruck;

    public delegate void DirectCamFocusDelegate();
    public static event DirectCamFocusDelegate EnableDirectorFocus, DisableDirectorFocus;
    #endregion

    //Pacing the level script
    #region LevelScript starters and stoppers
    //Whether the script should start at run
    public bool runScript;
    //Whether to pause and resume the script
    public static bool pauseScript = true;
    //Waiting until the current objective is complete
    public static bool waitTillObjectiveDone;
    public LevelRoutine thisLevelRoutine;
    #endregion 

    //Camera Data
    [Header("Camera Data")]
    public Vector3[] levelCameraPositions;
    public Vector3[] levelCameraAngles;

    //Audio Data + Interface
    [Header ("Audio Data")]
    public AudioMixer levelMixer;
    public AudioMixerSnapshot[] levelSnapshots;
    public AudioClip[] levelSfx;

    //Level Objects + Data
    [Header("Level Objects")]
    public GameObject player;
    public GameObject camera;

    public GameObject truck;
    public Transform[] truckPositions;

    public GameObject[] toggleableGeometry;
    public GameObject birds;

    public PickupArea plankPikcupArea;
    public PickupArea plankPutdownArea;
    public GameObject coverDropzone1;
    public GameObject secretBlockingPushable;
    public GameObject overHeadPuzzleViewCamPos;

    //Game-state Machine
    public static GamePlayState thisGameplayState = GamePlayState.Regular;
    public Playmode thisPlayMode = Playmode.Linear;

    public float truckSpeed = 3f;

    // Use this for initialization
    void Awake() {
        ObjectiveHandler.ObjDone += ObjectiveDoneListener;
    }
       
	void Start () {
        if (runScript == true)
            switch (thisLevelRoutine) {
                case LevelRoutine.CameraTesting:
                    StartCoroutine(CameraLevelRoutine());
                    break;
                case LevelRoutine.Truck:
                    StartCoroutine(TruckLevelCoroutine());
                    break;
            }
	}

    void ObjectiveDoneListener() {
        waitTillObjectiveDone = false;
    }

    //Script for the level
    IEnumerator CameraLevelRoutine() {
        //Starting off with regular player and camera control
        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        ResetCamPositionOnRig();
        EnableCameraInput();
        EnableCharacterInput();

        PlayerCamera.cameraState = camStates.STATE_DETACHED;
        PlayerCamera.camTar = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.detachedPosition = GameObject.Find("cam_detached_intro").transform.position;
        PlayerCamera.detachedFixedRotation = GameObject.Find("cam_detached_intro").transform.rotation;
        PlayerCamera.setRotationInstantlyNextFrame = true;

        print("waiting");
        AssignThisObjective("Hit this trigger", "", 3, "roomTrig1");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //Make it so that this is lerping to the position it'd have during regular player orbit

        PlayerCamera.cameraState = camStates.STATE_LERPDIRFOCUS;
        PlayerCamera.camTar = truck.transform;

        print("done waiting");

        print("hey");
        yield return new WaitForSeconds(3f);
        print("done");

        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
    }

    //Script for the level
    IEnumerator TruckLevelCoroutine()
    {
        //Set all relevant objects to their desired state at the beginning of the game
        truck.SetActive(false);
        levelSnapshots[0].TransitionTo(0f);
        EnableCharacterInput();
        EnableCameraInput();

        ///Intro Sequence - PC moves automatically, being tracked by the cam and transitioning to gameplay camera
        //Set up the camera for the sequence
        PlayerCamera.cameraState = camStates.STATE_DETACHED;
        PlayerCamera.camTar = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.detachedPosition = GameObject.Find("cam_detached_intro").transform.position;
        PlayerCamera.detachedFixedRotation = GameObject.Find("cam_detached_intro").transform.rotation;
        PlayerCamera.setRotationInstantlyNextFrame = true;

        //Tell the Player to move 
        yield return MovePlayer("movement_target_intro");
        //PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        //ResetCamPositionOnRig();
        //yield return MovePlayer("movement_target_intro2");

        //When they hit this trigger, spawn in the car
        //yield return AddAndWaitForObjective("Hit this trigger to spawn the truck", "", 3, "DropTrig1");

        //when they hit this trigger, make them wait until button press
        yield return AddAndWaitForObjective("Hit this trigger to prompt getting into cover", "", 3, "DropTrig2");
        
        //Make car show up, 
        truck.SetActive(true);
        StartCoroutine(MoveTruckAlongRail("truck_rail_1_start"));

        //Button Prompt Text
        PlayThisDialogue(13);

        //Pause until keypress
        Time.timeScale = 0;

        while (true) {
            if (Input.GetKeyDown(KeyCode.Q)) break;
            yield return null;
        }

        //Resume Gameplay and Move the Player Character to the target
        Time.timeScale = 1;
        yield return null; // shit breaks if i dont have this here, probs related to grounding or something, dont have time to debug
        yield return MovePlayer("drop_movement_target_1");

        // Enter cover
        var player = FindObjectOfType<PlayerController>();
        player.currentCover = coverDropzone1;
        player.coverCooldown = 1.2f;
        PlayerController.thisMoveState = MoveState.STATE_COVER;

        // Wait until were in cover
        while (PlayerController.thisMoveState != MoveState.STATE_COVER) yield return null;

        //Make car go away
        StartCoroutine(MoveTruckAlongRail("truck_rail_2"));

        //When they hit this trigger, make the car about to show up again
        //When they hit this trigger, make the car show up
        yield return AddAndWaitForObjective("Walk over the bridge", "", 3, "DropTrig3");
        StartCoroutine(MoveTruckAlongRail("truck_rail_3"));

        //Play text that tells the player tall cover blocks line of sight


        //When they hit this trigger, the player has dropped down into the maze room
        yield return AddAndWaitForObjective("Jump down the ledge", "", 3, "Drop2Trig1");

        //tell the player to get across

        //When the player hits the trigger before the broken bridge, say that the player will need to find a way across
        //yield return AddAndWaitForObjective("Jump down the ledge", "", 3, "Drop2Trig2");

        //When the player picks up the planks, start the alternating car section
        //If the cars see the player, they'll shoot, if they don't see the player for X seconds after showing up in either window, they'll move to the other window
        while(plankPikcupArea.pickable != null) yield return null;

        var truckLoopingRoutine = MoveTruckBackAndForth("truck_rail_looping_4", "truck_rail_looping_transition_in");
        StartCoroutine(truckLoopingRoutine);

        // Enter puzzle cam mode
        PlayerCamera.cameraState = camStates.STATE_DETACHED;
        PlayerCamera.camTar = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.detachedPosition = overHeadPuzzleViewCamPos.transform.position;
        PlayerCamera.detachedFixedRotation = overHeadPuzzleViewCamPos.transform.rotation;
        PlayerCamera.transitioning = true;

        //When the player has put down the bridge plank successfully, move the car to the close window
        while (plankPutdownArea.pickable == null) yield return null;

        StopCoroutine(truckLoopingRoutine);
        yield return null;
        StartCoroutine(MoveTruckAlongRail("truck_rail_5", false));

        //As the player finishes crossing the bridge, present an unskippable prompt
        //When the player presses the button for the prompt, move the player over to the right/back of the pushable yellow block.
        //The apc should be shooting towards the player throughout all of this, but cannot connect a hit because of the tall cover + the pushable is thick.
        yield return AddAndWaitForObjective("Walk over the bridge", "", 3, "Drop2Trig2");

        PlayThisDialogue(14);
        // Pause until keypress
        Time.timeScale = 0;

        while (true) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                break;
            }
            yield return null;
        }

        Time.timeScale = 1;
        yield return null; // shit breaks if i dont have this here, probs related to grounding or something, dont have time to debug

        yield return MovePlayer("movement_target_to_secret_pushable");

        // Set the player state to pushing
        player.coverCooldown = 1.2f;
        player.currentPush = secretBlockingPushable;
        PlayerController.thisMoveState = MoveState.STATE_PUSHING;
        PlayerCamera.detachedFixedRotation = Quaternion.identity;
        PlayerCamera.cameraState = camStates.STATE_PUSHING;

        // Wait until the cover has been pushed
        var originalCoverPos = secretBlockingPushable.transform.position;
        while (Vector3.Distance(originalCoverPos, secretBlockingPushable.transform.position) < 1f) yield return null;

        PlayerController.thisMoveState = MoveState.STATE_REGULAR;
        
        //The player should button press rapidly, or push the block. It should move slowly.
        //Once the block is out of the way, either move the player through the whole automatically, or disengage them so they can move through
        //The explosive shot should knock the block down to block the player from regressing
        /*
        //Current Maze room 
        AssignThisObjective("Find a way out", "", 3, "roomTrig1");

        PlayThisDialogue(1);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        
        print("before break");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        print("after break");
        DisableCameraInput();
        DisableCharacterInput();
        SetCharCamTransform(levelCameraPositions[1], levelCameraAngles[1]);
        truck.SetActive(true);
        truck.transform.position = truckPositions[0].position;
        this.GetComponent<AudioSource>().PlayOneShot(levelSfx[0], .3f);
        levelSnapshots[1].TransitionTo(.5f);
        yield return new WaitForSeconds(1f);
        PlayThisDialogue(2);
        yield return new WaitForSeconds(3f);

        PlayThisDialogue(3);        
        EnableCharacterInput();
        EnableCameraInput();
        ResetCamPositionOnRig();

        AssignThisObjective("Find the secret exit", "", 3, "truckTrig4");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //The player has navigated through the secret exit
        PlayThisDialogue(4);
        truck.SetActive(false);
        levelSnapshots[0].TransitionTo(1f);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);
        */

        //Secret Room
        AssignThisObjective("Continue", "", 3, "truckTrig2");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //The Player has hit the truck trigger
        //toggleableGeometry[0].SetActive(true);
        
        truck.SetActive(true);
        levelSnapshots[1].TransitionTo(.5f);
        PlayerCamera.cameraState = camStates.STATE_DIRFOCUS;
        DisableCharacterInput();
        PlayerCamera.camTar = truck.transform;
        currentSeg = 1;
        float counter = 0;
        while (counter < 2) {
            counter += Time.deltaTime;
            RailPlayer();
            yield return null;
        }

        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        EnableCharacterInput();
        PlayThisDialogue(5);

        AssignThisObjective("Run upstairs!", "", 3, "truckTrig9");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        //Player must collect the pieces
        AssignThisObjective("Collect 3 boxes", "", 1, "truckTrig6");
        waitTillObjectiveDone = true;

        PlayThisDialogue(6);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        while (waitTillObjectiveDone) { yield return null; }
        birds.GetComponent<Animation>().Play();

        PlayThisDialogue(7);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);
       
        //Player sets the bomb
        AssignThisObjective("Set the bomb by the broken window", "", 1, "truckTrig7");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        
        PlayThisDialogue(8);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        PlayThisDialogue(9);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        

        //Tell the Player to stand in front of the bench
        AssignThisObjective("Stand in front of the workbench", "", 3, "fallTrig1");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) {yield return null;}


        //Play camera sequence here
        toggleableGeometry[1].SetActive(false);

        //Wait till the player falls down
        AssignThisObjective("Fall down the hole", "", 3, "truckTrig5");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        ///New Approach with updated camera work

        // Start moving the truck
        var truckMovingRoutine = TruckMovingSubRoutine();
        StartCoroutine(truckMovingRoutine);

        PlayerCamera.cameraState = camStates.STATE_RAIL;
        PlayerCamera.camTar = truck.transform;
        PlayerCamera.followRail = GameObject.Find("rail1").transform;
        PlayerCamera.railOffset = -2;
        PlayerCamera.setRotationInstantlyNextFrame = true;

        // Wait for the player to land on the ground
        yield return new WaitForSeconds(0.75f);

        // Move the player towards the first cover
        PlayerController.thisMoveState = MoveState.STATE_SCRIPTEDMOVEMENT;
        PlayerController.scriptedMovementTarget = GameObject.Find("movement_target1").transform.position;

        //Tell the player they'll have to stay in cover
        PlayThisDialogue(10);
        GetComponent<AudioSource>().PlayOneShot(levelSfx[1], .8f);
        yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        AssignThisObjective("Move to the next piece of cover", "", 3, "chaseTrig1");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        PlayThisDialogue(11);
        //yield return new WaitForSeconds(DialogueHandler.currentTimeTillTextOff);

        AssignThisObjective("Make it to the end of the hallway", "", 3, "truckTrig3");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        StopCoroutine(truckMovingRoutine);
        //DDFocus();

        DisableTruck();
        PlayThisDialogue(12);
        truck.transform.position = truckPositions[1].position;
        levelSnapshots[2].TransitionTo(3f);

        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        PlayerCamera.transitioning = true;

        //when the player leaves
        AssignThisObjective("Run", "Get to the end of the corridor", 3, "truckTrig8");

        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //ThisDialogue(16);

        // Move the player towards the first cover
        PlayerController.thisMoveState = MoveState.STATE_SCRIPTEDMOVEMENT;
        PlayerController.scriptedMovementTarget = GameObject.Find("movement_target2").transform.position;

        PlayerCamera.cameraState = camStates.STATE_DETACHED;
        PlayerCamera.camTar = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.detachedPosition = Camera.main.transform.parent.position;

        yield return new WaitForSeconds(3f);
        yield return FadeOut();

        print("ey");
    }

    IEnumerator MovePlayer(string targetPosHelper) {
        var helper = GameObject.Find(targetPosHelper);

        PlayerController.thisMoveState = MoveState.STATE_SCRIPTEDMOVEMENT;
        PlayerController.scriptedMovementTarget = helper.transform.position;

        while(PlayerController.thisMoveState == MoveState.STATE_SCRIPTEDMOVEMENT) yield return null;
    }

    IEnumerator MoveTruckBackAndForth(string railName, string transitionRail) {
        yield return MoveTruckAlongRail(transitionRail, false);

        while (true) {
            yield return WaitForCoverDuration(3f);
            yield return MoveTruckAlongRail(railName);

            yield return WaitForCoverDuration(3f);
            yield return MoveTruckAlongRail(railName, true, true);
        }
    }

    IEnumerator WaitForCoverDuration(float duration) {
        float timeInCover = 0;

        while (true) {

            if(PlayerController.thisMoveState == MoveState.STATE_COVER) {
                timeInCover += Time.deltaTime;
                if (timeInCover > duration)
                    break;

            } else {
                timeInCover = 0;
            }

            yield return null;
        }
    }

    IEnumerator AddAndWaitForObjective(string objName, string objDesc, int objType, string objTargetNameBase) {
        AssignThisObjective(objName, objDesc, objType, objTargetNameBase);
        print("Assigned" + objName);
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
    }

    IEnumerator TruckMovingSubRoutine()
    {
        while (true)
        {
            truck.transform.position = new Vector3(player.transform.position.x, truck.transform.position.y, truck.transform.position.z);
            yield return null;
        }
    }

    // Dynamically creates a image that covers the entire screen
    IEnumerator FadeOut() {
        var screenCanvas = GameObject.Find("ScreenOverlayCanvas");
        if(screenCanvas == null) {
            Debug.LogError("Couldn't fade out, no overlay canvas by the name of ScreenOverlayCanvas found");
            yield break;
        }

        // Create and position the screenfader gameobject
        var imgObj = new GameObject("ScreenFader", typeof(RectTransform));
        var img = imgObj.AddComponent<Image>();
        imgObj.transform.SetParent(screenCanvas.transform, false);
        img.color = new Color(0, 0, 0, 0);

        // Position to cover the whole canvas
        var rectTransform = imgObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var color = img.color;
        for(float t  = 0; t < 1; t += Time.deltaTime) {
            color.a = Mathf.Lerp(0, 1, t);
            img.color = color;
            yield return null;
        }

        color.a = 1;
        img.color = color;
    }

    IEnumerator truckmovingRoutine;
    IEnumerator MoveTruckAlongRail(string rail, bool lerpToStart = true, bool reverse = false) {
        if (truckmovingRoutine != null)
            StopCoroutine(truckmovingRoutine);

        truckmovingRoutine = moveTruckAlongRail(rail, lerpToStart, reverse);
        return truckmovingRoutine;
    }

    IEnumerator moveTruckAlongRail(string rail, bool lerpToStart = true, bool reverse = false) {
        var r = GameObject.Find(rail);
        if(r == null) {
            Debug.LogError("Truck rail not found: " + rail);
        }
        if (reverse) {
            yield return r.GetComponent<Rail>().MoveObjectAlongRailReverse(truck.transform, truckSpeed, lerpToStart);
        } else {
            yield return r.GetComponent<Rail>().MoveObjectAlongRail(truck.transform, truckSpeed, lerpToStart);
        }
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

public enum LevelRoutine {
    Truck,
    CameraTesting
}
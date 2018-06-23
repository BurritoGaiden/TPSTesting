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

    public delegate void InputDelegate(bool desiredInput);
    public static event InputDelegate b_SetCharacterInput, b_SetCameraInput;

    public delegate void CameraStateDelegate(camStates desiredState);
    public static event CameraStateDelegate st_SetCameraState;

    public delegate void CameraDelegate();
    public static event CameraDelegate ResetCamPositionOnRig;

    public delegate void UIDelegate(string elementName, bool desiredInput);
    public static event UIDelegate b_SetUIElementEnabled;

    public delegate void CameraTransformDelegate(Transform desiredTransform, Quaternion desiredRotation);
    public static event CameraTransformDelegate SetCameraPosition;

    public delegate void InterestDelegate(String thisOne);
    public static event InterestDelegate EnableInterestTrigger, DisableInterestTrigger;

    public delegate void TruckDelegate();
    public static event TruckDelegate DisableTruck;

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

    public PickupArea plankPutdownArea;
    public GameObject coverDropzone1;
    public GameObject secretBlockingPushable;
    public GameObject overHeadPuzzleViewCamPos;
    public GameObject introCameraTarget;

    public Transform bridgeToEntrancePoint;
    public Transform EntrancePoint;

    public Transform EntranceCameraTarget;
    public Transform dilapidatedPoint;

    public GameObject test0Point;
    public GameObject test1Point;
    
    //Game-state Machine
    public static GamePlayState thisGameplayState = GamePlayState.Regular;
    public Playmode thisPlayMode = Playmode.Linear;

    public float truckSpeed = 3f;
    public AudioClip[] truckDialogue;

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
                case LevelRoutine.Quarantine:
                    StartCoroutine(QuarantineLevelCoroutine());
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
        b_SetCameraInput(true);
        b_SetCharacterInput(true);

        st_SetCameraState(camStates.STATE_DETACHED);
        PlayerCamera.targetPosition = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.detachedPosition = GameObject.Find("cam_detached_intro").transform.position;
        PlayerCamera.detachedFixedRotation = GameObject.Find("cam_detached_intro").transform.rotation;
        PlayerCamera.setRotationInstantlyNextFrame = true;

        //Tell the Player to move 
        PlayerCamera.transitionSpeed = 6f;
        yield return MovePlayer("movement_target_intro");


        print("waiting");
        AssignThisObjective("Hit this trigger", "", 3, "roomTrig1");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        //Make it so that this is lerping to the position it'd have during regular player orbit

        PlayerCamera.cameraState = camStates.STATE_LERPDIRFOCUS;
        PlayerCamera.targetPosition = introCameraTarget.transform;

        print("done waiting");

        print("hey");
        yield return new WaitForSeconds(3f);
        print("done");

        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
    }

    //Script for the level
    IEnumerator TruckLevelCoroutine()
    {
        print("Setup truck and music");
        //Set all relevant objects to their desired state at the beginning of the game
        truck.SetActive(false);
        b_SetUIElementEnabled("HealthVignette", false);

        print("set up inputs");
        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        ResetCamPositionOnRig();
        b_SetCharacterInput(true);
        b_SetCameraInput(true);

        print("setup camera vars");
        ///Intro Sequence - PC moves automatically, being tracked by the cam and transitioning to gameplay camera
        //Set up the camera for the sequence
        st_SetCameraState(camStates.STATE_DETACHED);
        PlayerCamera.targetPosition = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.detachedPosition = GameObject.Find("cam_detached_intro").transform.position;
        PlayerCamera.detachedFixedRotation = GameObject.Find("cam_detached_intro").transform.rotation;
        PlayerCamera.setRotationInstantlyNextFrame = true;

        print("tell the player to move");
        //Tell the Player to move 
        PlayerCamera.transitionSpeed = 6f;
        yield return MovePlayer("movement_target_intro");

        print("Wait till hitting move trig");
        AssignThisObjective("Hit this trigger", "", 3, "roomTrig1");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        st_SetCameraState(camStates.STATE_LERPDIRFOCUS);
        PlayerCamera.targetPosition = introCameraTarget.transform;

        yield return new WaitForSeconds(1.5f);
        print("done");

        st_SetCameraState(camStates.STATE_PLAYERORBIT);
        //GameObject.Find("CameraRig").GetComponent<CameraShake>().Shake(1, 1);

        //when they hit this trigger, make them wait until button press
        yield return AddAndWaitForObjective("Hit this trigger to prompt getting into cover", "", 3, "DropTrig2");
        
        //Make car show up, 
        truck.SetActive(true);
        truck.GetComponent<AudioSource>().volume = .2f;
        MoveTruckAlongRail("truck_rail_1_start", false);

        //Button Prompt Text
        PlayThisDialogue(13);

        b_SetUIElementEnabled("HealthVignette", true);

        //Pause until keypress
        Time.timeScale = 0.00001f;

        while (true) {
            if (Input.GetKeyDown(KeyCode.Q)) break;
            yield return null;
        }

        b_SetUIElementEnabled("HealthVignette", false);

        //Resume Gameplay and Move the Player Character to the target
        Time.timeScale = 1;
        yield return null; // shit breaks if i dont have this here, probs related to grounding or something, dont have time to debug
        yield return MovePlayerWait("drop_movement_target_1");

        // Enter cover
        var player = FindObjectOfType<PlayerController>();
        player.currentCover = coverDropzone1;
        player.coverCooldown = 1.2f;
        PlayerController.thisMoveState = MoveState.STATE_COVER;
        //tell the car to get louder
        truck.GetComponent<AudioSource>().volume = .5f;

        // Wait until were in cover
        while (PlayerController.thisMoveState != MoveState.STATE_COVER) yield return null;

        //Make car go away
        MoveTruckAlongRail("truck_rail_2");
        yield return new WaitForSeconds(3.5f);
        //tell the car to quiet down
        truck.GetComponent<AudioSource>().volume = .1f;

        //When they hit this trigger, make the car about to show up again
        yield return AddAndWaitForObjective("Walk over the bridge", "", 3, "DropTrig3");
        truck.GetComponent<AudioSource>().volume = .7f;
        MoveTruckAlongRail("truck_rail_3", false);

        waitTillObjectiveDone = true;
        AssignThisObjective("Jump down the ledge", "", 3, "Drop2Trig1");
        truck.GetComponent<AudioSource>().volume = .3f;

        // Wait for the player to enter cover, or jump down
        while (PlayerController.thisMoveState != MoveState.STATE_COVER && waitTillObjectiveDone) yield return null;

        // Move truck down 
        MoveTruckAlongRail("truck_rail_looping_transition_in");

        // WAit till the player jumps down
        while (waitTillObjectiveDone) yield return null;

        //When the player picks up the planks, start the alternating car section
        //If the cars see the player, they'll shoot, if they don't see the player for X seconds after showing up in either window, they'll move to the other window
        GameObject plankPickupArea = GameObject.Find("PlankPickupArea_Step15");
        while (plankPickupArea.GetComponent<PickupArea>().pickable != null) yield return null;
        //Destroy the crates here
        GameObject[] crates = GameObject.FindGameObjectsWithTag("PuzzleCrates");
        for (int i = 0; i < crates.Length; i++) {
            crates[i].GetComponent<CrateDestroyer>().ExplodeCrate();
        }

        var truckLoopingRoutine = MoveTruckBackAndForth("truck_rail_looping_4", "");
        StartCoroutine(truckLoopingRoutine);

        PlayerCamera.cameraState = camStates.STATE_PUZZLELERPDIRFOCUS;
        PlayerCamera.targetPosition = GameObject.FindWithTag("PuzzleCameraView").transform;
        PlayerCamera.cameraLookTarget = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.valuePlaceholder = GameObject.FindWithTag("PuzzlePlaceholder").transform;

        //When the player has put down the bridge plank successfully, move the car to the close window
        GameObject plankPutdownArea = GameObject.Find("PlankPutdownArea_Step16");
        while (plankPutdownArea.GetComponent<PickupArea>().pickable == null) yield return null;

        StopCoroutine(truckLoopingRoutine);
        truck.GetComponent<EnemyAPC>().SetTurretAimDir(TurretDirection.Forward);

        yield return null;
        MoveTruckAlongRail("truck_rail_5", false);

        truck.GetComponent<EnemyAPC>().thisAimState = APCAimState.STATE_PLAYER;

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

        //Tell camera to lerp to a specific point
        PlayerCamera.cameraState = camStates.STATE_LERPDIRFOCUS;
        PlayerCamera.targetPosition = EntranceCameraTarget;
        //PlayerCamera.currentView = EntrancePoint;

        Time.timeScale = 1;
        yield return null; // shit breaks if i dont have this here, probs related to grounding or something, dont have time to debug

        yield return MovePlayerWait("movement_target_to_secret_pushable");

        // Set the player state to pushing
        player.coverCooldown = 1.2f;
        player.currentPush = secretBlockingPushable;
        PlayerController.thisMoveState = MoveState.STATE_PUSHING;
        PlayerCamera.detachedFixedRotation = Quaternion.identity;
        //PlayerCamera.cameraState = camStates.STATE_PUSHING;

        // Wait until the cover has been pushed
        var originalCoverPos = secretBlockingPushable.transform.position;
        while (Vector3.Distance(originalCoverPos, secretBlockingPushable.transform.position) < 1f) yield return null;

        //Tell player to move through hole
        yield return MovePlayerWait("dilapidated_room");
        //camera.transform.position = dilapidatedPoint.position;
        //camera.transform.rotation = dilapidatedPoint.rotation;
        PlayerController.thisMoveState = MoveState.STATE_REGULAR;
        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;

        //Secret Room
        AssignThisObjective("Continue", "", 3, "truckTrig2");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }
        
        truck.SetActive(true);
        //levelSnapshots[1].TransitionTo(.5f);
        PlayerCamera.cameraState = camStates.STATE_DIRFOCUS;
        b_SetCharacterInput(false);
        PlayerCamera.targetPosition = truck.transform;

        truck.GetComponent<EnemyAPC>().PlayRail(rail, true);
        truck.GetComponent<EnemyAPC>().thisAimState = APCAimState.STATE_PLAYER;
        for (float counter = 0; counter < 2; counter += Time.deltaTime) {
            //counter += Time.deltaTime;
            //RailPlayer();
            yield return null;
        }

        PlayerCamera.cameraState = camStates.STATE_PLAYERORBIT;
        b_SetCharacterInput(true);
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

        truck.GetComponent<EnemyAPC>().thisAimState = APCAimState.STATE_PLAYER;

        PlayerCamera.cameraState = camStates.STATE_RAIL;
        PlayerCamera.targetPosition = truck.transform;
        PlayerCamera.followRail = GameObject.Find("rail1").transform;
        PlayerCamera.railOffset = -6;
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
        //levelSnapshots[2].TransitionTo(3f);

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
        PlayerCamera.targetPosition = GameObject.FindWithTag("Player").transform.Find("CameraTarget");
        PlayerCamera.detachedPosition = Camera.main.transform.parent.position;

        yield return new WaitForSeconds(3f);
        yield return FadeOut();

        print("ey");
    }

    IEnumerator SearchMode() {

        print("check true");
        bool coverChecking = true;
        float checkerSwitch = 3f;
        float passedThisSection = 0f;

        while (true)
        {
            //print("in loop");
            if (checkerSwitch > 0) checkerSwitch -= Time.deltaTime;
            //print(checkerSwitch);
            if (checkerSwitch <= 0)
            {
                coverChecking = !coverChecking;
                checkerSwitch = 3f;
                passedThisSection++;
                print(coverChecking);
            }

            yield return null;
        }
    }

    IEnumerator QuarantineLevelCoroutine() {
        //Setting up camera and player character
        print("Set up camera and Player");

        //st_SetCameraState(camStates.STATE_PLAYERCONTROLLEDRIG_REGULARCAM);
        //ResetCamPositionOnRig();

        b_SetCharacterInput(true);
        b_SetCameraInput(true);

        truck.GetComponent<Truck>().thisPerceptionState = TruckPerceptionState.nothing;

        st_SetCameraState(camStates.STATE_TARGETRIG_REGULARCAM);
        camera.GetComponent<PlayerCamera>().boomArmDisplacement = new Vector3(0, 0, -3);
        camera.GetComponent<PlayerCamera>().rigTarget = GameObject.Find("part0CameraTarget00").transform;
        camera.GetComponent<PlayerCamera>().camTarget = null;

        yield return new WaitForSeconds(1f);

        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part0CharPosition06").transform.position, 3f));

        yield return new WaitForSeconds(4f);

        StartCoroutine(VectorLerper(camera.GetComponent<PlayerCamera>().boomArmDisplacement, new Vector3(1, 0, 0), 1f));
        camera.GetComponent<PlayerCamera>().rigTarget = GameObject.Find("CharB").transform;

        yield return new WaitForSeconds(1f);
        
        StartCoroutine(VectorLerper(camera.GetComponent<PlayerCamera>().boomArmDisplacement, new Vector3(.43f, .18f, 1.4f), 2f));

        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part0CharPosition01").transform.position, 2f));

        yield return new WaitForSeconds(2.5f);

        StartCoroutine(LerpObjectToPosition(truck, GameObject.Find("part0TruckPosition01").transform.position, 1.5f));

        yield return new WaitForSeconds(.5f);

        st_SetCameraState(camStates.STATE_TARGETRIG_REGULARCAM);
        camera.GetComponent<PlayerCamera>().rigTarget = truck.transform;
        camera.GetComponent<PlayerCamera>().camTarget = null;

        yield return new WaitForSeconds(.25f);

        st_SetCameraState(camStates.STATE_PLAYERCONTROLLEDRIG_TARGETCAM);
        camera.GetComponent<PlayerCamera>().camTarget = truck.transform;

        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part0CharPosition02").transform.position, 3.5f));

        yield return new WaitForSeconds(.75f);

        StartCoroutine(LerpObjectToPosition(truck, truckPositions[0].position, 1f));
        

        yield return new WaitForSeconds(1f);
        StartCoroutine(VectorLerper(camera.GetComponent<PlayerCamera>().boomArmDisplacement, new Vector3(1, 0, 0), 2f));

        st_SetCameraState(camStates.STATE_TARGETRIG_TARGETCAM);
        camera.GetComponent<PlayerCamera>().rigTarget = truck.transform;
        camera.GetComponent<PlayerCamera>().camTarget = truck.transform;

        yield return new WaitForSeconds(1f);

        
        st_SetCameraState(camStates.STATE_PLAYERCONTROLLEDRIG_REGULARCAM);
        camera.GetComponent<PlayerCamera>().rigTarget = null;
        camera.GetComponent<PlayerCamera>().camTarget = null;
        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part0CharPosition03").transform.position, 2f));


        yield return new WaitForSeconds(2f);

        
        //-------------------------------------------------------------

        print("Part 1 - Tease vehicle");

        AssignThisObjective("Hit this trigger", "", 3, "part1Trigger00");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part1CharPosition01").transform.position, 3f));

        AssignThisObjective("Hit this trigger", "", 3, "part1Trigger01");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }



        print("Switching Camera to Focus");
        camera.GetComponent<PlayerCamera>().rigTarget = GameObject.Find("part1LookTarget01").transform;
        st_SetCameraState(camStates.STATE_TARGETRIG_REGULARCAM);

        print("Moving Truck");
        StartCoroutine(LerpObjectToPosition(truck, truckPositions[1].position, 4f));
        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStart");

        yield return new WaitForSeconds(2f);

        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part1CharPosition02").transform.position, 3f));

        st_SetCameraState(camStates.STATE_PLAYERCONTROLLEDRIG_REGULARCAM);

        yield return new WaitForSeconds(2f);

        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStop");

        //-------------------------------------------------------------------------

        print("Part 2 - Intro to Vehicle + Tease Vehicle Intention");

        truck.transform.position = truckPositions[2].position;

        AssignThisObjective("Hit this trigger", "", 3, "part2Trigger01");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        print("Moving Truck");
        StartCoroutine(LerpObjectToPosition(truck, truckPositions[3].position, 2f));
        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStart");
        

        yield return new WaitForSeconds(2f);

        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStop");
        GetComponent<AudioSource>().PlayOneShot(truckDialogue[1], 4);

        yield return new WaitForSeconds(2f);

        truck.GetComponent<Truck>().turret.GetComponent<Turret>().FaceObject(player.transform.position, 2f);

        yield return new WaitForSeconds(2f);


        StartCoroutine(LerpObjectToPosition(truck, truckPositions[4].position, 4f));
        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStart");

        yield return new WaitForSeconds(2f);

        //GetComponent<AudioSource>().PlayOneShot(truckDialogue[2], 4);

        yield return new WaitForSeconds(2f);

        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStop");
        GetComponent<AudioSource>().PlayOneShot(truckDialogue[4], 4);

        yield return new WaitForSeconds(2f);

        StartCoroutine(LerpObjectToPosition(truck, truckPositions[5].position, 3f));

        //-------------------------------------------------------------------------

        print("Part 3 - Vehicle Intention / Player Danger / Goal : Observed");

        //print("Switching Camera to Focus");
        //st_SetCameraState(camStates.STATE_DIRFOCUS);
        //PlayerCamera.cameraLookTarget = truck.transform;

        AssignThisObjective("Hit this trigger", "", 3, "part3Trigger00");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        print("Switching Camera to Focus");
        camera.GetComponent<PlayerCamera>().rigTarget = truck.transform;
        st_SetCameraState(camStates.STATE_TARGETRIG_REGULARCAM);

        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part3CharPosition00").transform.position, 3f));

        StartCoroutine(LerpObjectToPosition(truck, truckPositions[6].position, 2f));
        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStart");

        yield return new WaitForSeconds(2f);

        truck.GetComponent<Truck>().turret.GetComponent<Turret>().FaceObject(GameObject.Find("part3TurretTarget01").transform.position, 1.5f);

        GetComponent<AudioSource>().PlayOneShot(truckDialogue[5], 5);

        AssignThisObjective("Hit this trigger", "", 3, "part3Trigger01");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        float waiting = 0;
        float waitingPlus = 10;
        while (waiting < waitingPlus)
        {
            waiting += Time.deltaTime;
            print(waiting + " and the limit" + waitingPlus);
            truck.GetComponent<Truck>().turret.GetComponent<Turret>().Shoot();
            yield return null;
        }

        print("finished that previous shoot bit");

        print("Switching Camera to Regular");
        //camera.GetComponent<PlayerCamera>().rigTarget = truck.transform;
        st_SetCameraState(camStates.STATE_PLAYERCONTROLLEDRIG_REGULARCAM);

        yield return new WaitForSeconds(1f);

        StartCoroutine(LerpObjectToPosition(truck, truckPositions[7].position, 2f));
        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStart");

        StartCoroutine(LerpObjectToPosition(GameObject.Find("CharB"), GameObject.Find("part3CharPosition01").transform.position, 3f));
        
        yield return new WaitForSeconds(2);

        //-------------------------------------------------------------------------

        print("Part 4 - Vehicle pass with the Player having cover");

        //Player walks through this section
        //The player is in cover for part of this
        //the vehicle can be seen double tapping shapes that look like people
        //The player no longer has any cover
        //when they get to a certain point, the vehicle starts coming in quick
        //the player must run over to the cover
        //When near the cover, the game will prompt the player to enter it
        //Once entered, the vehicle passes by

        AssignThisObjective("Hit this trigger", "", 3, "part4Trigger01");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        truck.transform.position = GameObject.Find("part4TruckTarget01").transform.position;
        StartCoroutine(LerpObjectToPosition(truck, GameObject.Find("part4TruckTarget02").transform.position, 4f));
        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeStart");

        yield return new WaitForSeconds(4f);

        AssignThisObjective("Hit this trigger", "", 3, "part4Trigger02");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        StartCoroutine(LerpObjectToPosition(truck, GameObject.Find("part4TruckTarget03").transform.position, 10f));

        AssignThisObjective("Hit this trigger", "", 3, "part4Trigger03");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        //put button prompt here
        //Pause until keypress
        Time.timeScale = 0.00001f;

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Q)) break;
            yield return null;
        }

        Time.timeScale = 1;

        StartCoroutine(LerpObjectToPosition(truck, GameObject.Find("part4TruckTarget04").transform.position, 2f));
        
        //-------------------------------------------------------------------------

        print("Part 5 - Vehicle 'Search Mode'");

        AssignThisObjective("Hit this trigger", "", 3, "part5Trigger01");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        StartCoroutine(LerpObjectToPosition(truck, GameObject.Find("part5TruckTarget01").transform.position, 2f));

        truck.GetComponent<Truck>().searchpoints[0] = GameObject.Find("part5TurretTarget01");
        truck.GetComponent<Truck>().searchpoints[1] = GameObject.Find("part5TurretTarget02");
        truck.GetComponent<Truck>().searchpoints[2] = GameObject.Find("part5TurretTarget03");

        print(truck.GetComponent<Truck>().searchpoints[0].name);
        print(truck.GetComponent<Truck>().searchpoints[1].name);
        print(truck.GetComponent<Truck>().searchpoints[2].name);
        truck.GetComponent<Truck>().thisPerceptionState = TruckPerceptionState.searchingBetweenPoints;

        AssignThisObjective("Hit this trigger", "", 3, "part5Trigger02");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        //-------------------------------------------------------------------------

        print("Part 6 - Generator Minigame");

        AssignThisObjective("Hit this trigger", "", 3, "part6Trigger01");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Q)) break;
            player.GetComponent<PlayerController>().takeInput = false;
            yield return null;
        }

        print("Switching Camera to Focus");
        camera.GetComponent<PlayerCamera>().rigTarget = GameObject.Find("part6CameraLookTarget01").transform;
        st_SetCameraState(camStates.STATE_TARGETRIG_REGULARCAM);

        StartCoroutine(VectorLerper(camera.GetComponent<PlayerCamera>().boomArmDisplacement, new Vector3(-.5f, .18f, 1.1f), 2f));

        yield return new WaitForSeconds(2);

        print("Waiting till the Player presses Q to advance to the next part");
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Q)) break;
            yield return null;
        }

        StartCoroutine(VectorLerper(camera.GetComponent<PlayerCamera>().boomArmDisplacement, new Vector3(1f, .0f, 0f), 1f));

        camera.GetComponent<PlayerCamera>().rigTarget = GameObject.Find("part6CameraLookTarget02").transform;
        StartCoroutine(LerpObjectToFace(GameObject.Find("DoorHolder"), GameObject.Find("DoorHolderTarget").transform.position, 3));

        yield return new WaitForSeconds(.3f);

        player.GetComponent<PlayerController>().takeInput = true;

        yield return new WaitForSeconds(2.5f);

        st_SetCameraState(camStates.STATE_PLAYERCONTROLLEDRIG_REGULARCAM);
        truck.GetComponent<Truck>().thisPerceptionState = TruckPerceptionState.nothing;

        yield return new WaitForSeconds(5);

        //-------------------------------------------------------------------------

        print("Part 7 - Vehicle Crash into Wall");

        print("The Truck is about to crash into the wall");
        truck.transform.position = GameObject.Find("part7TruckTarget01").transform.position;

        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveeCrashIntoWall");

        yield return new WaitForSeconds(4.7f);

        //Destroy the crates here
        GameObject[] crates = GameObject.FindGameObjectsWithTag("PuzzleCrates");
        for (int i = 0; i < crates.Length; i++)
        {
            crates[i].GetComponent<CrateDestroyer>().ExplodeCrate();
        }

        truck.GetComponent<Truck>().thisPerceptionState = TruckPerceptionState.omniscientPerceived;

        print("The Player must get to the other side of the room, between bursts of bullets");
        AssignThisObjective("Hit this trigger", "", 3, "part7Trigger01");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        print("The player has made it to the other side");

        truck.transform.GetChild(3).GetComponent<Animation>().Play("HumveePullOutOfWall");

        //truck.transform.position = truckPositions[4].position;
        GetComponent<AudioSource>().PlayOneShot(truckDialogue[5], 4);

        yield return new WaitForSeconds(3f);

        print("it has pulled out");

        while (true)
        {
            Vector3 tempTruckVector = player.transform.position;
            tempTruckVector.y = truck.transform.position.y;
            tempTruckVector.x = truck.transform.position.x;
            tempTruckVector.z = player.transform.position.z;
            truck.transform.position = tempTruckVector;
            if(Mathf.Abs(player.transform.position.z) - Mathf.Abs(truck.transform.position.z) > 10)
            yield return null;
            break;
        }
        

        print("The Player must get to the end of the corridor while getting shot at");
        AssignThisObjective("Hit this trigger", "", 3, "part7Trigger02");
        waitTillObjectiveDone = true;
        while (waitTillObjectiveDone) { yield return null; }

        
    }

    IEnumerator VectorLerper(Vector3 theVector, Vector3 targetVector, float TimeToLerp) {
        float lerpStartTime = Time.time;
        float lerpTimeSinceStarted = Time.time - lerpStartTime;
        float lerpPercentageComplete = lerpTimeSinceStarted / TimeToLerp;
        Vector3 vectorStart = theVector;
        while (true)
        {
            lerpTimeSinceStarted = Time.time - lerpStartTime;
            lerpPercentageComplete = lerpTimeSinceStarted / TimeToLerp;
            print("Time since started: " + lerpTimeSinceStarted + " and Percent Complete" + lerpPercentageComplete);
            Vector3 currentVector = Vector3.Lerp(vectorStart, targetVector, lerpPercentageComplete); 
            camera.GetComponent<PlayerCamera>().boomArmDisplacement = currentVector;

            if (lerpPercentageComplete >= 1) break;
            yield return new WaitForEndOfFrame();
        }
        print("Done with Lerping: " + theVector);
    }

    IEnumerator MovePlayer(string targetPosHelper)
    {
        var helper = GameObject.Find(targetPosHelper);

        PlayerController.thisMoveState = MoveState.STATE_SCRIPTEDMOVEMENT;
        PlayerController.scriptedMovementTarget = helper.transform.position;
        yield return new WaitForSeconds(.1f);
    }

    IEnumerator MovePlayerWait(string targetPosHelper) {
        var helper = GameObject.Find(targetPosHelper);

        PlayerController.thisMoveState = MoveState.STATE_SCRIPTEDMOVEMENT;
        PlayerController.scriptedMovementTarget = helper.transform.position;

        while(PlayerController.thisMoveState == MoveState.STATE_SCRIPTEDMOVEMENT) yield return null;
    }

    IEnumerator MoveTruckBackAndForth(string railName, string transitionRail) {
        //If there's a transition rail, move along that first
        if (!string.IsNullOrEmpty(transitionRail)) {
            MoveTruckAlongRail(transitionRail, false);

            // Wait until it's done moving along the transition rail
            while (truck.GetComponent<EnemyAPC>().thisMoveState == APCMoveState.STATE_RAIL) yield return null;
        }

        while (true) {
            truck.GetComponent<EnemyAPC>().thisAimState = APCAimState.STATE_PLAYER;

            // Wait until we have been in cover for 3 seconds
            yield return WaitForCoverDuration(3f);

            truck.GetComponent<EnemyAPC>().SetTurretAimDir(TurretDirection.Forward);

            MoveTruckAlongRail(railName);

            // Wait until it's done moving along the rail
            while (truck.GetComponent<EnemyAPC>().thisMoveState == APCMoveState.STATE_RAIL) yield return null;

            truck.GetComponent<EnemyAPC>().thisAimState = APCAimState.STATE_PLAYER;

            // Wait until we have been in cover for 3 seconds
            yield return WaitForCoverDuration(3f);

            truck.GetComponent<EnemyAPC>().FlipDirection();
            truck.GetComponent<EnemyAPC>().SetTurretAimDir(TurretDirection.Forward);

            MoveTruckAlongRail(railName, true, true);

            // Wait until it's done moving along the rail
            while (truck.GetComponent<EnemyAPC>().thisMoveState == APCMoveState.STATE_RAIL) yield return null;

            truck.GetComponent<EnemyAPC>().FlipDirection();
        }

    }

    IEnumerator LerpObjectToPosition(GameObject desiredObject, Vector3 desiredPosition, float timeToLerp) {
        float lerpStartTime = Time.time;
        float lerpTimeSinceStarted = Time.time - lerpStartTime;
        float lerpPercentageComplete = lerpTimeSinceStarted / timeToLerp;
        Vector3 ObjectStartPosition = desiredObject.transform.position;
        while (true) {
            lerpTimeSinceStarted = Time.time - lerpStartTime;
            lerpPercentageComplete = lerpTimeSinceStarted / timeToLerp;
            print("Time since started: " + lerpTimeSinceStarted + " and Percent Complete" + lerpPercentageComplete);
            Vector3 currentPosition = Vector3.Lerp(ObjectStartPosition, desiredPosition, lerpPercentageComplete);
            //Vector3 currentPosition = desiredPosition - desiredObject.transform.position 
            desiredObject.transform.position = currentPosition;

            if (lerpPercentageComplete >= 1) break;
            yield return new WaitForEndOfFrame();
        }
        print("Done with Lerping: " + desiredObject);
    }

    IEnumerator LerpObjectToFace(GameObject desiredObject, Vector3 desiredTarget, float timeToLerp) {
        float lerpStartTime = Time.time;
        float lerpTimeSinceStarted = Time.time - lerpStartTime;
        float lerpPercentageComplete = lerpTimeSinceStarted / timeToLerp;

        
        Vector3 relativePos = desiredTarget - desiredObject.transform.position;
        //relativePos.y = desiredObject.transform.position.y;
        //relativePos.z = desiredObject.transform.position.z;

        print(desiredObject.transform.position);
        print(desiredTarget);
        print(relativePos);

        Quaternion startingRot = desiredObject.transform.rotation;
        Quaternion desiredRotation = Quaternion.LookRotation(relativePos);

        while (true)
        {
            lerpTimeSinceStarted = Time.time - lerpStartTime;
            lerpPercentageComplete = lerpTimeSinceStarted / timeToLerp;

            Quaternion currentRot = Quaternion.Lerp(startingRot, desiredRotation, lerpPercentageComplete);
            desiredObject.transform.rotation = currentRot;

            if (lerpPercentageComplete >= 1) break;
            yield return new WaitForEndOfFrame();
        }
        print("Done lerping Quaternion");
    }

    public IEnumerator FadeCanvasGroup(Image cg, float start, float end, float lerpTime = .5f)
    {
        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (true)
        {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            cg.color = new Vector4(cg.color.r, cg.color.g, cg.color.b, currentValue);

            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }
        print("done with opacity adjust");
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

            Debug.Log("HMMMM");
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

    void MoveTruckAlongRail(string rail, bool lerpToStart = true, bool reverse = false) {
        var r = GameObject.Find(rail);
        if (r == null) {
            Debug.LogError("Truck rail not found: " + rail);
        }

        if (reverse) {
            truck.GetComponent<EnemyAPC>().PlayRailReverse(r.GetComponent<Rail>(), !lerpToStart);
        } else {
            truck.GetComponent<EnemyAPC>().PlayRail(r.GetComponent<Rail>(), !lerpToStart);
        }
    }

    public Rail rail;
    //TODO: program a delegate that allows calls for specific targets on an objective being completed
}

public enum GamePlayState {
    Regular,
    Combat,
    Finale
}

public enum LevelRoutine {
    Truck,
    CameraTesting,
    Quarantine
}
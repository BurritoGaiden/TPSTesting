using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour {

    public GameObject[] wheels;

    public bool spinWheels;

    public TruckPerceptionState thisPerceptionState = TruckPerceptionState.nothing;
    public TurretState thisTurretState = TurretState.nothing;

    Vector3 target = GameObject.Find("Character").transform.position;

    Quaternion from, to;
    float timeToFace = 0f;

    public bool playerSpotted;
    public float playerSpotTime;
    public bool playerPerceived;

    public Vector3 lastKnownPlayerPosition;

    public 

	// Update is called once per frame
	void Update () {
        //Movement
        float x = 0;
        float y = 0;
        float z = 0;

        if (spinWheels) {
            x = 1;
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetComponent<Wheel>().Spin(true, 50);
        }

        if (Input.GetKey(KeyCode.N)) {
            wheels[0].GetComponent<Wheel>().TurnWheels();
            wheels[1].GetComponent<Wheel>().TurnWheels();
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            wheels[0].GetComponent<Wheel>().InitiateTurn(new Vector3(1, 0, 180), 3f);
            wheels[1].GetComponent<Wheel>().InitiateTurn(new Vector3(30, 1, 1), 3f);
        }

        

        //Perception
        Perception(5f);

        switch (thisPerceptionState) {
            case TruckPerceptionState.nothing:

                break;
            case TruckPerceptionState.inomniscientPerceived:

                break;
            case TruckPerceptionState.omniscientPerceived:

                break;
            case TruckPerceptionState.searchingBetweenPoints:

                break;
            case TruckPerceptionState.trainedOnAPoint:

                break;

        }

        switch (thisTurretState) {
            case TurretState.nothing:

                break;

            case TurretState.faceTarget:
                //Get current turret rotation
                Quaternion temp = turret.transform.rotation;
                from = Quaternion.Euler(new Vector3(0, temp.eulerAngles.y, 0));

                //Get rotation based on desiredTarget position
                Vector3 relativePos = target - turret.transform.position;
                Quaternion desiredRotation = Quaternion.LookRotation(relativePos);

                //Assign desired Rotation without other axes
                to = Quaternion.Euler(new Vector3(0, desiredRotation.eulerAngles.y, 0));
                timeToFace = 0;
                break;

            case TurretState.faceForward:

                break;
        }

        //Apply turret changes
        timeToFace += Time.deltaTime;
        if (thisTurretState == TurretState.faceForward || thisTurretState == TurretState.faceTarget)
        {
            //This is what allows for the rotation changes
            turret.transform.rotation = Quaternion.Lerp(from, to, timeToFace);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Shoot();
        }

        //About Face 90 degrees
        if (Input.GetKeyDown(KeyCode.RightControl)) {
        print(turret.transform.rotation);
        Quaternion temp = turret.transform.rotation;
        from = Quaternion.Euler(new Vector3(0,temp.eulerAngles.y,0));
        to = Quaternion.Euler(from.eulerAngles + new Vector3(0, 90f, 0));

        timeToFace = 0;
        }
    }

    public int ammo;
    public float fireCooldownTimer;

    public LineRenderer lr;
    public GameObject turret;
    public GameObject impactEffect;

    public delegate void DamageDelegate();
    public static event DamageDelegate HitPlayer;

    void Start() {
        turret = transform.GetChild(3).GetChild(0).gameObject;
        lr = GetComponent<LineRenderer>();
    }

    void Shoot()
    {
        ammo--;

        fireCooldownTimer = Random.Range(.1f, .2f);
        //GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .02f);

        // Check what we hit
        RaycastHit hit;

        var distance = Vector3.Distance(turret.transform.position, turret.transform.forward);

        var hitPoint = Vector3.zero;
        if (!Physics.Linecast(turret.transform.position, turret.transform.position + turret.transform.forward * distance, out hit, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            hitPoint = turret.transform.position + turret.transform.forward * distance;
        }
        else
        {
            hitPoint = hit.point;
        }

        lr.SetPosition(0, turret.transform.position);
        lr.SetPosition(1, hitPoint);
        Invoke("TurnOffTrail", .1f);

        if (hit.collider)
        {
            if (hit.transform.tag == "Player")
            {
                print("hit player");
                HitPlayer();

            }
        }
        else if (hit.collider != null)
        {
            // Hit something other than the player, like the map
            GameObject impactObj = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactObj, 1.2f);

            print("hit something else");
        }
    }

    void TurnOffTrail() {
        lr.SetPosition(1, turret.transform.position);
    }

    //If Searching for the Player
    void Perception(float secondsTillPerceived) {
        ///Perception

        //cast a line from the turret to the Player
        Vector3 relativePos = target - turret.transform.position;
        Vector3 normalizedTargetPos = new Vector3(target.x, turret.transform.position.y, target.z);
        Vector3 relativeNormalizedTargetPos = normalizedTargetPos - turret.transform.position;

        RaycastHit playerHit;
        var pDistance = Vector3.Distance(turret.transform.position, turret.transform.forward);
        var playerHitPoint = Vector3.zero;
        Physics.Linecast(turret.transform.position, target, out playerHit);
        
        Debug.DrawLine(turret.transform.position, normalizedTargetPos, Color.red);
        Debug.DrawLine(turret.transform.position, target, Color.magenta);
        
        //cast a line from the turret forward
        RaycastHit forwardHit;

        var fDistance = Vector3.Distance(turret.transform.position, turret.transform.forward);
        var forwardHitPoint = Vector3.zero;
        if (!Physics.Linecast(turret.transform.position, turret.transform.position + turret.transform.forward * fDistance, out forwardHit, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            forwardHitPoint = turret.transform.position + turret.transform.forward * fDistance;
        else
            forwardHitPoint = forwardHit.point;

        Debug.DrawLine(turret.transform.position, forwardHitPoint, Color.green);

        //if that line is within x of the forward vector line for the mg
        float angle = Vector3.Angle(relativeNormalizedTargetPos, turret.transform.forward);
        //If that line hits the player
        if (angle < 15 && playerHit.collider)
        { playerSpotted = (playerHit.collider.name == "Character");
        }
        else { playerSpotted = false; }
        

        // In/De-crement time spent with Player spotted
        if (playerSpotted)
        {
            playerSpotTime += Time.deltaTime; 
            if (playerSpotTime > 4) //If the player is spotted for x seconds
            { 
                playerPerceived = true; //The Player is perceived
                lastKnownPlayerPosition = GameObject.Find("Character").transform.position;
            }
        }
        else {
            if (playerSpotTime > 0) {
                playerSpotTime -= Time.deltaTime * 2;
                if (playerSpotTime < 0) {
                    playerSpotTime = 0;
                }
            }
        }   
    }

    //Slot this into a state machine, or the level script idc
    //it should run until the player is seen
    void TurretSearch(GameObject[] searchPositions) {
        if (searchPositions != null)
        {
            //Put all that jazz from below in here
        }
        else {
            print("Turret can't search, dummy");
        }

        //Maybe an entrance behavior below
        //If not narrow spotlight
            //Narrow Turret Spotlight

        //Pass search positions to the function, x time to wait on each position, and y speed of turret rotation
        //Start with position 0
        //For each position
            //If not facing that position
                //rotate to face it
                //return until facing
            //If facing that position
                //If x seconds haven't elapsed
                    //Wait for x seconds
                    //return
                //If x seconds have elapsed
                    //if at end of array
                        //reset to start of array / 0
                    //if not at end of array
                        //continue
    }

    //You need to better separate out the behaviors in here and put em in a state
    //This is for if the TurretSearch just got the Player
    void Perceived(bool canLosePlayerPosition) {
        //this is probably going to have to be multiple little sub states
        //splitting them into a few pieces of behavior

        //Maybe an entrance behavior below
        //if turret spotlight not wide
        //Wide turret spotlight

        //If the turret is omnipotent
        //If turret spotlight not "all"
        //"all" turret spotlight
        //Point the turret at the player position
        //If the Player within x of forward Turret vector
        //If shoot possible
        //shoot

        //if the turret is not omnipotent
        if (true) { 
            //If the player not within x of forward turret vector
            if (!playerSpotted)
            {
                thisTurretState = TurretState.faceTarget;
                target = lastKnownPlayerPosition;
                //Point turret to last "known" player position
                //If z time has elapsed
                    //No longer in "perceived"
                    //break out of all of this, really

            }

            //If the player within x of forward Turret vector
            else //If the player can be seen
            {
                //Point the turret at the Player position
                thisTurretState = TurretState.faceTarget;                    
                    //If shoot Possible

                        //Shoot
            }
        }
    }
}

public enum TurretState {
    nothing, //Turret should just do nothing, and be ready to take one off input
    faceForward, //Turret should face forward, used while driving
    faceTarget //Turret should face a target
};
public enum TruckPerceptionState {
    nothing, //Truck not perceiving anything
    searchingBetweenPoints, //Truck searching between different points
    trainedOnAPoint, //Truck watching a singular spot
    omniscientPerceived, //Truck always knows where the Player is. Will probably aim towards that position 
    inomniscientPerceived //Truck doesn't always know where the Player is, and will attempt to keep them in their sight
};
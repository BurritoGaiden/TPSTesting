using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour {

    public GameObject[] wheels;

    public bool spinWheels;
	

	// Update is called once per frame
	void Update () {
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

        if (Input.GetKeyDown(KeyCode.K)) {
            Shoot();
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

    //Turn the turret toward a thing
    void TurnTurretToFace(GameObject theThing, float timeToFace = .7f) {

    }

    void TurnOffTrail() {
        lr.SetPosition(1, turret.transform.position);
    }

    //If Searching for the Player
    void Perception(float secondsTillPerceived) {
        ///Perception
        //cast a line from the turret to the Player

        //if that line is within x of the forward vector line for the mg
            //If that line hits the player
                //The Player is spotted


        //If the player is spotted for x seconds
            //The Player is perceived
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
            
            //If the player not within x of forward turret vector
                //Point turret to last "known" player position
                //If z time has elapsed
                    //No longer in "perceived"
                    //break out of all of this, really
            //If the player within x of forward Turret vector
                //If the player can be seen
                    //Point the turret at the Player position
                    //If shoot Possible
                        //Shoot
    }
}

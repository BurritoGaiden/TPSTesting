using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour {

    public GameObject[] wheels;
    public GameObject turret;

    public bool spinWheels;

    public TruckPerceptionState thisPerceptionState = TruckPerceptionState.nothing;

    public GameObject targetObject;
    Vector3 targetObjectLastKnownPosition;

    public bool playerSpotted;
    public float playerSpotTime;
    public bool playerPerceived;

    public Vector3 lastKnownPlayerPosition;
    public float timePlayerLastSeen;

    void Start()
    {
        turret = transform.GetChild(3).GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update () {
        WheelMovement();

        Perception(targetObject);

        switch (thisPerceptionState) {
            case TruckPerceptionState.nothing:
                //Full control from level script
                break;

            case TruckPerceptionState.inomniscientPerceived:
                turret.GetComponent<Turret>().spotlight.GetComponent<Light>().spotAngle = 45;

                if (!playerPerceived) //If the player not within x of forward turret vector
                {
                    turret.GetComponent<Turret>().FaceTarget(lastKnownPlayerPosition); //Point turret to last "known" player position
                    turret.GetComponent<Turret>().RotationUpdate(2);

                    if (!playerPerceived) {
                        thisPerceptionState = TruckPerceptionState.trainedOnAPoint;
                    }
                }

                //If the player is seen
                else if (playerPerceived) {
                    turret.GetComponent<Turret>().FaceTarget(targetObject.transform.position); //Point the turret at the Player position
                    turret.GetComponent<Turret>().RotationUpdate(4);
                    turret.GetComponent<Turret>().Shoot(); //Let the turret know it can shoot
                }

                break;

            case TruckPerceptionState.omniscientPerceived:
                turret.GetComponent<Turret>().spotlight.GetComponent<Light>().spotAngle = 180; //Truck can see everything. Reflect that in visuals

                turret.GetComponent<Turret>().FaceTarget(targetObject.transform.position); //Point the turret at the Player position
                turret.GetComponent<Turret>().RotationUpdate(4);
                turret.GetComponent<Turret>().Shoot(); //Let the turret know it can shoot

                break;
            case TruckPerceptionState.searchingBetweenPoints:
                turret.GetComponent<Turret>().spotlight.GetComponent<Light>().spotAngle = 30;

                if (playerPerceived) {
                    thisPerceptionState = TruckPerceptionState.inomniscientPerceived;
                }
                break;
            case TruckPerceptionState.trainedOnAPoint:
                turret.GetComponent<Turret>().spotlight.GetComponent<Light>().spotAngle = 30;

                if (playerPerceived)
                    thisPerceptionState = TruckPerceptionState.inomniscientPerceived;
                break;
        }
    }   
    
    //Whether the Player is seen or not
    void Perception(GameObject perceivedObject) {
        ///Perception

        //cast a line from the turret to the Player
        Vector3 relativePos = perceivedObject.transform.position - turret.transform.position;
        Vector3 normalizedTargetPos = new Vector3(perceivedObject.transform.position.x, turret.transform.position.y, perceivedObject.transform.position.z);
        Vector3 relativeNormalizedTargetPos = normalizedTargetPos - turret.transform.position;

        RaycastHit playerHit;
        var pDistance = Vector3.Distance(turret.transform.position, turret.transform.forward);
        var playerHitPoint = Vector3.zero;
        Physics.Linecast(turret.transform.position, perceivedObject.transform.position, out playerHit);
        
        //Debug.DrawLine(turret.transform.position, normalizedTargetPos, Color.red);
        Debug.DrawLine(turret.transform.position, perceivedObject.transform.position, Color.magenta);
        
        //cast a line from the turret forward
        RaycastHit forwardHit;
        var fDistance = Vector3.Distance(turret.transform.position, turret.transform.forward);
        var forwardHitPoint = Vector3.zero;
        if (!Physics.Linecast(turret.transform.position, turret.transform.position + turret.transform.forward * fDistance, out forwardHit, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            forwardHitPoint = turret.transform.position + turret.transform.forward * fDistance;
        else
            forwardHitPoint = forwardHit.point;

        Debug.DrawLine(turret.transform.position, forwardHitPoint, Color.green);

        //Check if the targetObject is within X degrees of the forward turret vector, and is unobstructed
        float angle = Vector3.Angle(relativeNormalizedTargetPos, turret.transform.forward);

        if (angle < 15 && playerHit.collider)
            playerSpotted = (playerHit.collider.name == perceivedObject.name);
        else
            playerSpotted = false; 
        

        // Increment time spent with Player spotted
        if (playerSpotted)
        {
            if(playerSpotTime < 4)
                playerSpotTime += Time.deltaTime; 
            if (playerSpotTime > 2) //If the player is spotted for x seconds
            { 
                playerPerceived = true; //The Player is perceived
                lastKnownPlayerPosition = perceivedObject.transform.position;
            }
        }
        else {
            playerPerceived = false;
            if (playerSpotTime > 0) {
                playerSpotTime -= Time.deltaTime * 2;
                if (playerSpotTime < 0) {
                    playerSpotTime = 0;
                }
            }
        }   
    }

    void WheelMovement()
    {
        //Movement
        float x = 0;
        float y = 0;
        float z = 0;

        if (spinWheels)
        {
            x = 1;
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetComponent<Wheel>().Spin(true, 50);
        }

        if (Input.GetKey(KeyCode.N))
        {
            wheels[0].GetComponent<Wheel>().TurnWheels();
            wheels[1].GetComponent<Wheel>().TurnWheels();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            wheels[0].GetComponent<Wheel>().InitiateTurn(new Vector3(1, 0, 180), 3f);
            wheels[1].GetComponent<Wheel>().InitiateTurn(new Vector3(30, 1, 1), 3f);
        }
    }

}

public enum TruckPerceptionState {
    nothing, //Truck not perceiving anything
    searchingBetweenPoints, //Truck searching between different points
    trainedOnAPoint, //Truck watching a singular spot
    omniscientPerceived, //Truck always knows where the Player is. Will probably aim towards that position 
    inomniscientPerceived //Truck doesn't always know where the Player is, and will attempt to keep them in their sight
};
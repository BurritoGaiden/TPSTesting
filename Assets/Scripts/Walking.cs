using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walking : MonoBehaviour {

    //Declare delegates for the player collecting objects, because it's not the player subclasses job to track collection

    /// Rules
    /// Needs to know what object it should be collecting through some sort of listener
    /// Needs to then send out an event when those collectables have been collected

    //Sample received delegate string
    public string lookingFor;

    //Declaring the delegate
    public delegate void WalkDelegate(GameObject walkable);
    public static event WalkDelegate WalkThis;

    void Awake() {
        ObjectiveHandler.targetName += UpdateLookingFor;
    }

    //Declaring the delegate
    void UpdateLookingFor(string looking) {
        lookingFor = looking;
    }

    //Check for the collision. Send a delegate out for the collided object, make sure to use OnControllerColliderHit if using CharacterController
    void OnControllerColliderHit(ControllerColliderHit col)
    {
        //We can assume that objects that are targets will have names long enough than the shortest target name base
        if (col.gameObject.tag == "Target")
        {
            string stringToCheckAgainst = col.gameObject.name;

            stringToCheckAgainst = stringToCheckAgainst.Substring(0, lookingFor.Length);

            if (stringToCheckAgainst == lookingFor)
            {
                WalkThis(col.gameObject);
            }
        }
    }

    //Check for the collision. Send a delegate out for the collided object, make sure to use OnControllerColliderHit if using CharacterController
    void OnTriggerEnter(Collider col)
    {
        //We can assume that objects that are targets will have names long enough than the shortest target name base
        if (col.gameObject.tag == "Target")
        {
            string stringToCheckAgainst = col.gameObject.name;

            stringToCheckAgainst = stringToCheckAgainst.Substring(0, lookingFor.Length);

            if (stringToCheckAgainst == lookingFor)
            {
                WalkThis(col.gameObject);
            }
        }
    }
}

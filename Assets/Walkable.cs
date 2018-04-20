using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walkable : MonoBehaviour {

    //Current Data
    public bool on;

    //Interface
    public BoxCollider thisBoxCollider;
    public MeshRenderer thisMeshRenderer;

    void Start()
    {
        on = true;
        thisBoxCollider = this.GetComponent<BoxCollider>();
        thisMeshRenderer = this.GetComponent<MeshRenderer>();
        Walking.WalkThis += DisableWalkable;
    }

    //Collectables shouldn't need to check if they're being collided with
    public void DisableWalkable(GameObject thisObject)
    {
        if (thisObject == this.gameObject)
        {
            on = false;
            thisBoxCollider.enabled = false;
            Walking.WalkThis -= DisableWalkable;
        }
    }

    public void EnableWalkable()
    {
        on = true;
        thisBoxCollider.enabled = true;
        thisMeshRenderer.enabled = true;
    }

}

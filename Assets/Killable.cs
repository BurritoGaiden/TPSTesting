using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killable : MonoBehaviour {

    //Current Data
    public bool on;

    //Interface
    public BoxCollider thisBoxCollider;
    public MeshRenderer thisMeshRenderer;

    void Awake()
    {
        on = true;
        thisBoxCollider = this.GetComponent<BoxCollider>();
        thisMeshRenderer = this.GetComponent<MeshRenderer>();

        Killing.killThis += DisableKillable;
    }

    //Collectables shouldn't need to check if they're being collided with

    public void DisableKillable(GameObject thisObject)
    {
        if (thisObject == this.gameObject)
        {
            on = false;
            thisBoxCollider.enabled = false;
            thisMeshRenderer.enabled = false;
            Collecting.collectThis -= DisableKillable;
        }
    }

    public void EnableKillable()
    {
        on = true;
        thisBoxCollider.enabled = true;
        thisMeshRenderer.enabled = true;
    }

}

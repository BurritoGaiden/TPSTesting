using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Collectable : MonoBehaviour {

    //Current Data
    public bool on;

    //Interface
    public BoxCollider thisBoxCollider;
    public MeshRenderer thisMeshRenderer;

    void Start() {
        on = true;
        thisBoxCollider = this.GetComponent<BoxCollider>();
        thisMeshRenderer = this.GetComponent<MeshRenderer>();

        Collecting.collectThis += DisableCollectable;
    }

    //Collectables shouldn't need to check if they're being collided with

    public void DisableCollectable(GameObject thisObject) {
        if (thisObject == this.gameObject)
        {
            on = false;
            thisBoxCollider.enabled = false;
            thisMeshRenderer.enabled = false;
            Collecting.collectThis -= DisableCollectable;
        }
    }

    public void EnableCollectable() {
        on = true;
        thisBoxCollider.enabled = true;
        thisMeshRenderer.enabled = true;
    }
	
}
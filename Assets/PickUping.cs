using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUping : MonoBehaviour {

    public bool canPickup;
    public bool hasObject;
    public float pickCooldown;

    public GameObject pickable;

	// Use this for initialization
	void Start () {
        hasObject = false;
        canPickup = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (pickCooldown > 0) {
            pickCooldown -= Time.deltaTime;
        }
	}

    void OnTriggerStay(Collider col) {
        if (col.GetComponent<PickupArea>()) {
            //print("in area");
            if (Input.GetKeyDown(KeyCode.Q)) {
                if (pickCooldown <= 0) {
                    ProcessPickup(col.gameObject);
                }
            }
        }
    }

    void ProcessPickup(GameObject pickupArea) {

        if (hasObject)
        {
            Putdown(pickupArea);
            pickCooldown = 3;
        }
        else
        {
            if (pickupArea.GetComponent<PickupArea>().pickable)
            {
                Pickup(pickupArea);
                pickCooldown = 3;
            }
        }
    }

    void Pickup(GameObject pickupArea) {
        hasObject = true;
        pickable = pickupArea.GetComponent<PickupArea>().pickable;
        pickupArea.GetComponent<PickupArea>().pickable = null;
        pickable.GetComponent<BoxCollider>().enabled = false;
        pickable.GetComponent<MeshRenderer>().enabled = false;
        Debug.Log("has object");
    }

    void Putdown(GameObject pickupArea) {
        hasObject = false;
        if (pickable.GetComponent<Pickupable>().destinationArea == pickupArea) {
            pickable.GetComponent<BoxCollider>().enabled = true;
            pickable.GetComponent<MeshRenderer>().enabled = true;

            pickable.transform.position = pickable.GetComponent<Pickupable>().slotPosition;
            pickable.transform.rotation = pickable.GetComponent<Pickupable>().slotRotation;

            pickable = null;
            Debug.Log("does not have object");
            return;
        }
        pickupArea.GetComponent<PickupArea>().pickable = pickable;
        pickable = null;
        pickupArea.GetComponent<PickupArea>().pickable.GetComponent<BoxCollider>().enabled = true;
        pickupArea.GetComponent<PickupArea>().pickable.GetComponent<MeshRenderer>().enabled = true;
        pickupArea.GetComponent<PickupArea>().pickable.transform.position = pickupArea.transform.position;
    }
}

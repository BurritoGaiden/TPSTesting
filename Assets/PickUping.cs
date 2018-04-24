using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUping : MonoBehaviour {

    public float pickCooldown;
    public GameObject pickable;
	
	// Update is called once per frame
	void Update () {
        if (pickCooldown > 0) {
            pickCooldown -= Time.deltaTime;
        }
	}

    void OnTriggerStay(Collider col) {
        if (col.GetComponent<PickupArea>()) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                if (pickCooldown <= 0) {
                    if (pickable) Putdown(col.gameObject);
                    else
                    {
                        if (col.GetComponent<PickupArea>().pickable)
                        {
                            Pickup(col.gameObject);  
                        }
                    }
                }
            }
        }
    }

    void Pickup(GameObject pickupArea) {
        pickable = pickupArea.GetComponent<PickupArea>().pickable;
        pickupArea.GetComponent<PickupArea>().pickable = null;
        pickable.GetComponent<BoxCollider>().enabled = false;
        pickable.GetComponent<MeshRenderer>().enabled = false;
        Debug.Log("has object");
        pickCooldown = 3;
    }

    void Putdown(GameObject pickupArea) {
        pickable.GetComponent<BoxCollider>().enabled = true;
        pickable.GetComponent<MeshRenderer>().enabled = true;

        pickable.transform.position = pickupArea.GetComponent<PickupArea>().pickablePlacementPlaceholder.transform.position;
        pickable.transform.rotation = pickupArea.GetComponent<PickupArea>().pickablePlacementPlaceholder.transform.rotation;

        if (!pickable.GetComponent<Pickupable>().destinationArea == pickupArea)
        {
            pickupArea.GetComponent<PickupArea>().pickable = pickable;
        }

        pickable = null;
        Debug.Log("does not have object");
        pickCooldown = 3;
    }
}

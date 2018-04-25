using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUping : MonoBehaviour {

    float pickCooldown;
    public GameObject pickable;
    public Image pickImage;

    void Start()
    {
        pickImage.enabled = false;
    }

    // Update is called once per frame
    void Update () {
        if (pickCooldown > 0) {
            pickCooldown -= Time.deltaTime;
        }
	}

    void OnTriggerStay(Collider col) {
        //Visual
        if (col.GetComponent<PickupArea>())
        {
            if (col.GetComponent<PickupArea>().pickable)
            {
                pickImage.enabled = true;
                pickImage.rectTransform.position = new Vector3(col.transform.position.x, col.transform.position.y + 1f, col.transform.position.z);
            }
        }
        else return;

        if (Input.GetKeyDown(KeyCode.Q) && pickCooldown <= 0)
        {
            //If the Player has an object and the pickup area is empty
            if (pickable && !col.GetComponent<PickupArea>().pickable) Putdown(col.gameObject);
            //If the Player doesn't have an object and the area does
            else if (!pickable && col.GetComponent<PickupArea>().pickable) Pickup(col.gameObject);
        }
        
    }

    void OnTriggerExit(Collider hit)
    {
        if (hit.GetComponent<PickupArea>())
        {
            pickImage.enabled = false;
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

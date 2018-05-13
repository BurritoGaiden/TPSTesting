using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUping : MonoBehaviour {

    float pickCooldown;
    public static GameObject pickable;
    public Image pickImage;
    public Text pickText;

    void Start()
    {
        pickImage.enabled = false;
        pickText.enabled = false;
    }

    // Update is called once per frame
    void Update () {
        if (pickCooldown > 0) {
            pickCooldown -= Time.deltaTime;
        }
        if(pickImage.enabled)
        pickImage.transform.LookAt(pickImage.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        pickText.transform.LookAt(pickText.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }

    void OnTriggerStay(Collider col) {
        //Visual
        if (col.GetComponent<PickupArea>() == null) return;

        var pickupArea = col.GetComponent<PickupArea>();
        if (pickupArea.pickable && pickupArea.canPickupFrom)
        {
            pickImage.enabled = true;
            pickImage.rectTransform.position = new Vector3(col.transform.position.x, col.transform.position.y + 1f, col.transform.position.z);
            pickText.enabled = true;
            pickText.rectTransform.position = new Vector3(col.transform.position.x, col.transform.position.y + 1.5f, col.transform.position.z);
            pickText.text = "Press E to Pick Up";
        }
        else if (!pickupArea.pickable && pickable)
        {
            pickImage.rectTransform.position = new Vector3(col.transform.position.x, col.transform.position.y + 1f, col.transform.position.z);
            pickImage.enabled = true;
            pickText.enabled = true;
            pickText.text = "Press E to Put Down";
            pickText.rectTransform.position = new Vector3(col.transform.position.x, col.transform.position.y + 1.5f, col.transform.position.z);
        }
        else if (!pickupArea.pickable && !pickable) {
            pickImage.enabled = false;
            pickText.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.E) && pickCooldown <= 0)
        {
            //If the Player has an object and the pickup area is empty
            if (pickable && !pickupArea.pickable) Putdown(col.gameObject);
            //If the Player doesn't have an object and the area does
            else if (!pickable && pickupArea.pickable && pickupArea.canPickupFrom) Pickup(col.gameObject);
        }
    }

    void OnTriggerExit(Collider hit)
    {
        if (hit.GetComponent<PickupArea>())
        {
            pickImage.enabled = false;
            pickText.enabled = false;
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


        pickupArea.GetComponent<PickupArea>().pickable = pickable;

        pickable = null;
        Debug.Log("does not have object");
        pickCooldown = 3;
    }
}

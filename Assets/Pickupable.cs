using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour {

    public Vector3 originPosition;
    public GameObject destinationArea;
    public Vector3 slotPosition;
    public Quaternion slotRotation;
    public Transform thisB;

	// Use this for initialization
	void Start () {
        thisB = this.transform;
        slotPosition = transform.position;
        slotRotation = transform.rotation;
        //slotRotation = Quaternion.Euler(transform.rotation);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour {

    public Quaternion thisQ;

	// Use this for initialization
	void Start () {
        thisQ = this.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
        //print(this.transform.rotation.z);
	}
}

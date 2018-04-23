using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interesting : MonoBehaviour {

    public static bool canLook;

	// Update is called once per frame
	void Update () {
        if (canLook) print("can look");
	}

    void OnTriggerStay(Collider hit)
    {
        if (hit.transform.GetComponent<Interestable>()) {
            canLook = true;
        }
    }
    /*
    void OnTriggerExit(Collider hit) {
        if (hit.transform.GetComponent<Interestable>()) {
            canLook = false;
        }
    }
    */
}

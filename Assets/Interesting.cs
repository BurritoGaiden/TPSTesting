using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interesting : MonoBehaviour {

    public static bool looking;

	void OnTriggerStay(Collider hit)
    {
        if (hit.transform.GetComponent<Interestable>()) {
            looking = Input.GetKey(KeyCode.E);
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

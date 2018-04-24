using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interesting : MonoBehaviour {

    public static bool looking;
    public Image intImage;

    void Start() {
        intImage.enabled = false;
        looking = false;
    }
    
    void OnTriggerExit(Collider hit) {
        if (hit.transform.GetComponent<Interestable>())
        {
            looking = false;
            intImage.enabled = false;
        }
    }

    void OnTriggerStay(Collider hit)
    {
        if (hit.transform.GetComponent<Interestable>())
        {
            looking = Input.GetKey(KeyCode.E);
            intImage.enabled = !looking;
        }
    }
    
}

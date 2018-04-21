using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killing : MonoBehaviour {

    //Shoot raycast things
    public float damage = 10f;
    public float range = 100f;
    public Camera cam;

    //Sample received delegate string
    public string lookingFor;

    //Declaring the delegate
    public delegate void KillDelegate(GameObject killable);
    public static event KillDelegate killThis;

    // Use this for initialization
    void Awake () {
        cam = Camera.main;
        ObjectiveHandler.targetName += UpdateLookingFor;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Shoot();
        }
	}

    void Shoot() {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range)) {
            string stringToCheckAgainst = hit.transform.gameObject.name;
            stringToCheckAgainst = stringToCheckAgainst.Substring(0, lookingFor.Length);

            if (stringToCheckAgainst == lookingFor)
            {
                killThis(hit.transform.gameObject);
            }
        }
    }

    //Declaring the delegate
    void UpdateLookingFor(string looking)
    {
        lookingFor = looking;
    }
}

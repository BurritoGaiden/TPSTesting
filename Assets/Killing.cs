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

    public static bool canShoot;
    public static bool aiming;

    // Use this for initialization
    void Awake () {
        cam = Camera.main;
        ObjectiveHandler.targetName += UpdateLookingFor;
        LevelScript.EnableCharacterInput += EnableShoot;
        LevelScript.DisableCharacterInput += DisableShoot;
    }
	
	// Update is called once per frame
	void Update () {
        if (canShoot)
        {
            aiming = Input.GetKey(KeyCode.Mouse1);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shoot();
            }
        }
	}

    void Shoot() {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range)) {
            string stringToCheckAgainst = hit.transform.gameObject.name;
            if (lookingFor.Length > stringToCheckAgainst.Length) return;
            stringToCheckAgainst = stringToCheckAgainst.Substring(0, lookingFor.Length);

            if (stringToCheckAgainst == lookingFor)
            {
                killThis(hit.transform.gameObject);
            }
        }
    }

    void Aim() { }

    //Declaring the delegate
    void UpdateLookingFor(string looking)
    {
        lookingFor = looking;
    }

    void EnableShoot() {
        canShoot = true;
    }

    void DisableShoot() {
        canShoot = false;
    }
}

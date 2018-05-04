using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAPC : MonoBehaviour {

    public GameObject player;
    public GameObject turret;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        //float range = 10f;

        Debug.DrawLine(turret.transform.position, player.transform.position);

        if (Physics.Linecast(turret.transform.position, player.transform.position, out hit)) {
            if (hit.transform.tag == "Player") {
                //print("hit em");
            }
        }
        /*
        if (Physics.Raycast(turret.transform.position, player.transform.position, out hit, range))
        {
            if (hit.transform.tag == "Player") {
                print("hit player");
            }
        }
        */
    }
}

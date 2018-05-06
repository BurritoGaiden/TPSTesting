using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAPC : MonoBehaviour {

    public GameObject player;
    public GameObject turret;
    public float counter = 0;

    public AudioClip[] truckSFX;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        //float range = 10f;

        Debug.DrawLine(turret.transform.position, new Vector3(player.transform.position.x,player.transform.position.y + 1f, player.transform.position.z));

        if (Physics.Linecast(turret.transform.position, player.transform.position, out hit)) {
            if (hit.transform.tag == "Player" && counter <= 0) {
                //print("hit em");
                counter = .8f;
                GetComponent<AudioSource>().PlayOneShot(truckSFX[0], 3);
            }
        }

        if (counter > 0) {
            counter -= Time.deltaTime;
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

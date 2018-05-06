using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAPC : MonoBehaviour {

    public GameObject player;
    public GameObject turret;
    public float counter = 0;
    public float ammo = 0;

    public AudioClip[] truckSFX;

    public delegate void DamageDelegate();
    public static event DamageDelegate HitPlayer;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        //float range = 10f;

        Debug.DrawLine(turret.transform.position, new Vector3(player.transform.position.x,player.transform.position.y + 1f, player.transform.position.z));

        if (Physics.Linecast(turret.transform.position, player.transform.position, out hit)) {
            if (hit.transform.tag == "Player" && counter <= 0 && ammo > 0) {
                //print("hit em");
                ammo--;
                counter = Random.Range(.1f, .4f);
                GetComponent<AudioSource>().PlayOneShot(truckSFX[0], 2);
                float y = Random.Range(0, 3);
                if (y == 2)
                {
                    HitPlayer();
                }
            }
        }

        if (counter > 0) {
            counter -= Time.deltaTime;
        }

        if (ammo <= 0) {
            Invoke("GiveAmmo", 3);
            counter = 3;
        }
    }

    void GiveAmmo() {
        ammo = 20;
    }
}

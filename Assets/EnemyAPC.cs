using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAPC : MonoBehaviour {

    public GameObject player;
    public GameObject turret;
    public float counter = 0;
    public float ammo = 0;
    bool alive = true;

    public AudioClip[] truckSFX;

    public delegate void DamageDelegate();
    public static event DamageDelegate HitPlayer;

    public Rigidbody tracer;
    public float tracerSpeed;
    public GameObject impactEffect;
    public GameObject bulletTrail;
    public LineRenderer lr;

    void Awake() {
        LevelScript.DTruck += DisableTruck;
        lr = bulletTrail.GetComponent<LineRenderer>();
        
    }

    void TurnOnTrail() {
        bulletTrail.SetActive(true);
        //lr.SetPosition(0, turret.transform.localPosition);
        lr.SetPosition(1, new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z) * -1);
        Invoke("TurnOffTrail", .1f);
    }

    void TurnOffTrail() {
        lr.SetPosition(0, new Vector3(turret.transform.position.x, turret.transform.position.y + 1.5f, turret.transform.position.z) - this.transform.position);
        lr.SetPosition(1, new Vector3(turret.transform.position.x, turret.transform.position.y + 1.5f, turret.transform.position.z) - this.transform.position);
    }

    //TODO: Remove raycast from update, make a reoccurring function
    // Update is called once per frame
    void Update () {
        if (!alive)
            return;
        RaycastHit hit;
        RaycastHit[] coverHits;

        Debug.DrawLine(turret.transform.position, new Vector3(player.transform.position.x,player.transform.position.y + 1f, player.transform.position.z));

        if (Physics.Linecast(turret.transform.position, player.transform.position, out hit))
        {
            if (hit.transform.tag == "Player" && counter <= 0 && ammo > 0)
            {
                ammo--;
                counter = Random.Range(.3f, .5f);
                GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .02f);

                HitPlayer();
                lr.SetPosition(0, new Vector3(turret.transform.position.x, turret.transform.position.y + 1.5f, turret.transform.position.z) - this.transform.position);
                lr.SetPosition(1, new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z) - this.transform.position);
                Invoke("TurnOffTrail", .05f);

            }

            else if (hit.transform.tag == "Cover" && counter <= 0 && ammo > 0)
            {
                ammo--;
                counter = Random.Range(.3f, .35f);
                GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .02f);
                print("hit cover");
  
                GameObject impactObj = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactObj, 1.2f);

                lr.SetPosition(0, new Vector3(turret.transform.position.x, turret.transform.position.y + 1.5f, turret.transform.position.z) - this.transform.position);
                lr.SetPosition(1, new Vector3(hit.point.x, hit.point.y + 1f, hit.point.z) - this.transform.position);
                Invoke("TurnOffTrail", .05f);
            }

            else if (hit.transform.tag == "Geo" && counter <= 0 && ammo > 0)
            {
                print("hit geo");
               
            }

            else if (hit.transform.tag == "Shooter" && counter <= 0 && ammo > 0)
            {
                print("It's hitting the gun");
                
            }
        }
        
        if (counter > 0) {
            counter -= Time.deltaTime;
        }

        if (ammo <= 0) {
            Invoke("GiveAmmo", 3);
            counter = 2;
        }
    }

    void DisableTruck() {
        alive = false;
    }

    void GiveAmmo() {
        ammo = 20;
    }
}

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

    void Awake() {
        LevelScript.DTruck += DisableTruck;
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
                GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .5f);
                int hitChanceMax = 2;
                if (PlayerController.runInput)
                {
                    hitChanceMax = 3;
                }
                print(hitChanceMax);
                int y = Random.Range(0, hitChanceMax);
                print(y);
                if (y == 2)
                {
                    HitPlayer();
                }

                //Spawn the tracer
                Rigidbody instantiatedTracer = Instantiate(tracer, transform.position, transform.rotation) as Rigidbody;

                //Add velocity to the tracer
                instantiatedTracer.velocity = transform.TransformDirection(player.transform.position * -tracerSpeed);
            }

            else if (hit.transform.tag == "Cover" && counter <= 0 && ammo > 0)
            {
                ammo--;
                counter = Random.Range(.08f, .35f);
                GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .5f);
                print("hit cover");
                //Spawn the tracer
                Rigidbody instantiatedTracer = Instantiate(tracer, transform.position, transform.rotation) as Rigidbody;

                GameObject impactObj = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactObj, 1.2f);
                //Add velocity to the tracer
                instantiatedTracer.velocity = transform.TransformDirection(player.transform.position * -tracerSpeed);
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

        coverHits = Physics.RaycastAll(turret.transform.position, player.transform.position, 30f);
        for (int i = 0; i < coverHits.Length; i++)
        {
            RaycastHit coverHit = coverHits[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();
        }


        if (counter > 0) {
            counter -= Time.deltaTime;
        }

        if (ammo <= 0) {
            Invoke("GiveAmmo", 3);
            counter = 3;
        }
    }

    void DisableTruck() {
        alive = false;
    }

    void GiveAmmo() {
        ammo = 20;
    }
}

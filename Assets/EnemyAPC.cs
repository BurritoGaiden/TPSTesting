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

    public Rigidbody tracer;
    public float tracerSpeed;

    // Update is called once per frame
    void Update () {
        RaycastHit hit;
        RaycastHit[] coverHits;
        //float range = 10f;

        Debug.DrawLine(turret.transform.position, new Vector3(player.transform.position.x,player.transform.position.y + 1f, player.transform.position.z));
        //Debug.DrawLine(turret.transform.position, new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z));

        if (Physics.Linecast(turret.transform.position, player.transform.position, out hit)) {
            if (hit.transform.tag == "Player" && counter <= 0 && ammo > 0) {
                ammo--;
                counter = Random.Range(.08f, .35f);
                GetComponent<AudioSource>().PlayOneShot(truckSFX[0], 1);
                int hitChanceMax = 3;
                if (PlayerController.runInput) {
                    hitChanceMax = 4;
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
        }

        coverHits = Physics.RaycastAll(turret.transform.position, player.transform.position, 30f);
        for (int i = 0; i < coverHits.Length; i++)
        {
            RaycastHit coverHit = coverHits[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();
            print(coverHit);
            /*
            if (rend)
            {
                // Change the material of all hit colliders
                // to use a transparent shader.
                rend.material.shader = Shader.Find("Transparent/Diffuse");
                Color tempColor = rend.material.color;
                tempColor.a = 0.3F;
                rend.material.color = tempColor;
            }
            */
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

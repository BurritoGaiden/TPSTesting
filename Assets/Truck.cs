using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour {

    public GameObject[] wheels;

    public bool spinWheels;
	

	// Update is called once per frame
	void Update () {
        float x = 0;
        float y = 0;
        float z = 0;

        if (spinWheels) {
            x = 1;
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetComponent<Wheel>().Spin(true, 50);
        }

        if (Input.GetKey(KeyCode.N)) {
            wheels[0].GetComponent<Wheel>().TurnWheels();
            wheels[1].GetComponent<Wheel>().TurnWheels();
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            wheels[0].GetComponent<Wheel>().InitiateTurn(new Vector3(1, 0, 180), 3f);
            wheels[1].GetComponent<Wheel>().InitiateTurn(new Vector3(30, 1, 1), 3f);
        }

    }


    public int ammo;
    public float fireCooldownTimer;

    public LineRenderer lr;
    public GameObject turret;
    public GameObject impactEffect;

    public delegate void DamageDelegate();
    public static event DamageDelegate HitPlayer;

    void Start() {
        turret = transform.GetChild(3).GetChild(0).gameObject;
    }

    void Shoot()
    {

        ammo--;

        fireCooldownTimer = Random.Range(.1f, .2f);
        //GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .02f);

        // Check what we hit
        RaycastHit hit;

        var distance = Vector3.Distance(turret.transform.position, turret.transform.forward);

        var hitPoint = Vector3.zero;
        if (!Physics.Linecast(turret.transform.position, turret.transform.position + turret.transform.forward * distance, out hit, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            hitPoint = turret.transform.position + turret.transform.forward * distance;
        }
        else
        {
            hitPoint = hit.point;
        }

        lr.SetPosition(0, turret.transform.position);
        lr.SetPosition(1, hitPoint);
        Invoke("TurnOffTrail", .1f);

        if (hit.transform.tag == "Player")
        {
            HitPlayer();

        }
        else if (hit.collider != null)
        {
            // Hit something other than the player, like the map
            GameObject impactObj = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactObj, 1.2f);

            print("hit something else");
        }
    }
}

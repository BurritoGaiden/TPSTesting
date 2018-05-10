using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAPC : MonoBehaviour {

    public GameObject player;
    public GameObject turret;
    public float fireCooldownTimer = 0;
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


    bool reloading;

    void Awake() {
        LevelScript.DisableTruck += DisableTruck;
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

        var targetPos = new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z);

        // When were in certain states, target a little lower
        if (PlayerController.thisMoveState == MoveState.STATE_COVER || 
            PlayerController.thisMoveState == MoveState.STATE_CROUCH || 
            PlayerController.thisMoveState == MoveState.STATE_COVERAIM) {

            targetPos.y -= 0.5f;
        }

        Debug.DrawLine(turret.transform.position, targetPos);

        if (fireCooldownTimer <= 0 && ammo > 0)
            Shoot(targetPos);
        
        if (fireCooldownTimer > 0) {
            fireCooldownTimer -= Time.deltaTime;
        }

        if (ammo <= 0 && !reloading) {
            Invoke("GiveAmmo", 5);
            reloading = true;
            fireCooldownTimer = 5;
        }
    }

    void Shoot(Vector3 targetPos) {

        ammo--;

        fireCooldownTimer = Random.Range(.3f, .5f);
        GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .02f);

        // Check what we hit
        RaycastHit hit;

        var hitPoint = Vector3.zero;
        if (!Physics.Linecast(turret.transform.position, targetPos, out hit, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
            hitPoint = targetPos;
        } else {
            hitPoint = hit.point;
        }

        lr.SetPosition(0, turret.transform.position);
        lr.SetPosition(1, hitPoint);
        Invoke("TurnOffTrail", .1f);
        
        if (hit.transform.tag == "Player") {
            HitPlayer();

        } else if (hit.collider != null) {
            // Hit something other than the player, like the map
            GameObject impactObj = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactObj, 1.2f);

            print("hit something else");
        }
    }

    void DisableTruck() {
        alive = false;
    }

    void GiveAmmo() {
        reloading = false;
        ammo = 15;
    }
}

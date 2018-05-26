using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAPC : MonoBehaviour {

    public GameObject player;

    [Header("Movement")]
    public float acceleration = 1;
    public float maxSpeed = 1;
    public float turnSpeed = 50;
    float currentVelocity;


    [Header("Shooting")]
    public GameObject turret;
    public float fireCooldownTimer = 0;
    public float ammo = 0;
    bool alive = true;

    public AudioClip[] truckSFX;

    public delegate void DamageDelegate();
    public static event DamageDelegate HitPlayer;

    public float playerSight;
    public Rigidbody tracer;
    public float tracerSpeed;
    public GameObject impactEffect;
    public GameObject bulletTrail;
    public LineRenderer lr;

    public APCState thisAPCState = APCState.STATE_SEARCHING;
    public APCAimState thisAimState = APCAimState.STATE_LOCALDIRECTION;
    public APCMoveState thisMoveState = APCMoveState.STATE_NONE;

    public Rail currentRail;
    int currentRailNodeIndex;
    bool reverseRail; // move along the nodes on the rail in reverse

    public Quaternion targetLocalTurretRotation;

    bool reloading;

    APCMoveState lastMoveState;
    bool jerking = false;
    float lastVelocity;

    void Awake() {
        LevelScript.DisableTruck += DisableTruck;
        lr = bulletTrail.GetComponent<LineRenderer>();
        //this.GetComponent<AudioSource>().Play();
        //this.GetComponent<AudioSource>().loop = true;
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
            PlayerController.thisMoveState == MoveState.STATE_COVERAIM)
        {

            targetPos.y -= 0.5f;
        }

        UpdateTurretAim(targetPos);
        UpdateShooting(targetPos);
        UpdateMovement();
    }

    void UpdateShooting(Vector3 targetPos) {
        Debug.DrawLine(turret.transform.position, targetPos);

        switch (thisAPCState) {
            case APCState.STATE_SEARCHING:
                // Check what we hit
                RaycastHit playerCheck;

                var hitPoint = Vector3.zero;
                if (Physics.Linecast(turret.transform.position, targetPos, out playerCheck, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
                    if (playerCheck.transform.tag == "Player") {
                        if (playerSight < 3) {
                            playerSight += Time.deltaTime;
                        }
                    } else {
                        if (playerSight > 0) {
                            playerSight -= Time.deltaTime;
                        }
                    }
                }
                if (playerSight > 2) {
                    thisAPCState = APCState.STATE_SPOTTED;
                }
                break;
            case APCState.STATE_SPOTTED:
                if (fireCooldownTimer <= 0 && ammo > 0 && thisAimState == APCAimState.STATE_PLAYER)
                    Shoot(targetPos);

                if (ammo <= 0 && !reloading) {
                    Invoke("GiveAmmo", 5);
                    reloading = true;
                    fireCooldownTimer = 5;
                }

                break;
            case APCState.STATE_CHASING:

                break;
        }

        if (fireCooldownTimer > 0) {
            fireCooldownTimer -= Time.deltaTime;
        }
    }

    void UpdateTurretAim(Vector3 targetPos) {
        switch (thisAimState) {
            case APCAimState.STATE_LOCALDIRECTION: // Rotate to a fixed local direction
                turret.transform.localRotation = Quaternion.RotateTowards(turret.transform.localRotation, targetLocalTurretRotation, Time.deltaTime * 75);
                break;
            case APCAimState.STATE_PLAYER: // Rotate towards player
                RotateTurret(Quaternion.LookRotation(targetPos - turret.transform.position));
                break;
        }
    }

    void RotateTurret(Quaternion targetRot) {
        turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, targetRot, Time.deltaTime * 75);
    }

    
    void UpdateMovement() {
        switch (thisMoveState) {
            case APCMoveState.STATE_NONE:
                Decelerate();
                transform.position = transform.position + transform.forward * currentVelocity * Time.deltaTime;

                if (lastMoveState != APCMoveState.STATE_NONE) {
                    StartCoroutine(JerkRoutine());
                }

                lastMoveState = APCMoveState.STATE_NONE;
                break;
            case APCMoveState.STATE_RAIL:
                UpdateRailMovement();
                lastMoveState = APCMoveState.STATE_RAIL;
                break;
        }

        lastVelocity = currentVelocity;
    }

    void UpdateRailMovement() {
        if (jerking)
            return;

        var targetNode = currentRail.nodes[currentRailNodeIndex];
        var targetRot = Quaternion.LookRotation(targetNode.position - transform.position);

        // Update rotation
        var delta = targetNode.position - transform.position;
        var angle = Vector2.SignedAngle(Vector2.up, new Vector2(delta.x, delta.z));

        transform.rotation = Quaternion.RotateTowards(transform.rotation,  Quaternion.Euler(new Vector3(0, -angle, 0)), Time.deltaTime*turnSpeed);

        var startingUp = currentVelocity == 0;

        // Update velocity and position
        // Only increase velocity if our target is ahead
        if (Vector3.Angle(targetNode.position - transform.position, transform.forward) < 45) {
            Accelerate();
        } else {
            Decelerate();
        }

        if(startingUp && lastMoveState != APCMoveState.STATE_RAIL) {
            StartCoroutine(JerkRoutine());
            return;
        }
        
        transform.position = transform.position + transform.forward * currentVelocity * Time.deltaTime;

        // Move towards the target on the y axis
        // If the truck needs to roll up hills and such this and the rotation needs to be changed
        // But for now to keep it simple it will only rotate on the y axis and move easily along the y axis like this
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetNode.position.y, transform.position.z), Time.deltaTime * 5);

        // If were at our target node, change our target to the next node
        if(Vector3.Distance(transform.position, targetNode.position) < 0.1f) {
            if (reverseRail) {
                currentRailNodeIndex--;
                if (currentRailNodeIndex < 0) {
                    thisMoveState = APCMoveState.STATE_NONE;
                }
            } else {
                currentRailNodeIndex++;
                if (currentRailNodeIndex >= currentRail.nodes.Length) {
                    thisMoveState = APCMoveState.STATE_NONE;
                }
            }

            CheckRailChangeTurretState(currentRailNodeIndex);
        }
    }

    

    IEnumerator JerkRoutine() {
        jerking = true;
        //Play startup sfx here

        var originalPos = transform.position;

        var speed = 75f; //how fast it shakes
        var amount = 0.05f; //how much it shakes

        var add = new Vector3();
        for(float t = 0; t < 0.25; t += Time.deltaTime) {
            var movedElseWhere = transform.position - (originalPos + add);
            originalPos += movedElseWhere;

            add.x = Mathf.Sin(t * speed) * amount;
            add.y = Mathf.Sin((t + 1) * speed * 1.2f) * amount;
            add.z = Mathf.Sin((t - 1) * speed * 1.1f) * amount;

            speed *= 0.9f;

            transform.position = originalPos + add;
            yield return null;
        }

        transform.position = originalPos;

        jerking = false;
    }

    void Accelerate() {
        currentVelocity = Mathf.MoveTowards(currentVelocity, maxSpeed, acceleration * Time.deltaTime);
    }

    void Decelerate() {
        currentVelocity = Mathf.MoveTowards(currentVelocity, 0, Time.deltaTime * acceleration * 10);
    }

    void Shoot(Vector3 targetPos) {

        ammo--;

        fireCooldownTimer = Random.Range(.1f, .2f);
        GetComponent<AudioSource>().PlayOneShot(truckSFX[0], .02f);

        // Check what we hit
        RaycastHit hit;

        var distance = Vector3.Distance(turret.transform.position, targetPos);

        var hitPoint = Vector3.zero;
        if (!Physics.Linecast(turret.transform.position, turret.transform.position + turret.transform.forward * distance, out hit, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
            hitPoint = turret.transform.position + turret.transform.forward * distance;
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
        print("Gave Ammo");
        ammo = 15;
    }

    public float TurretDirectionToAngle(TurretDirection dir) {
        switch (dir) {
            case TurretDirection.Backward:
                return 180;
            case TurretDirection.Forward:
                return 0;
            case TurretDirection.Left:
                return -90;
            case TurretDirection.Right:
                return 90;
        }

        return 0;
    }

    // Set the movement state to move along a rail
    public void PlayRail(Rail rail, bool teleportToFirstNode = false) {
        currentRail = rail;
        currentRailNodeIndex = 0;
        if (teleportToFirstNode) {
            transform.position = rail.nodes[0].position;
            transform.LookAt(rail.nodes[1].position);
            currentRailNodeIndex = 1;
        }
        CheckRailChangeTurretState(currentRailNodeIndex);

        reverseRail = false;
        thisMoveState = APCMoveState.STATE_RAIL;
    }

    // Set the movement state to move along a rail in reverse
    public void PlayRailReverse(Rail rail, bool teleportToFirstNode = false) {
        currentRail = rail;
        currentRailNodeIndex = rail.nodes.Length-1;
        if (teleportToFirstNode) {
            transform.position = rail.nodes[currentRailNodeIndex].position;
            transform.LookAt(rail.nodes[currentRailNodeIndex-1].position);
            currentRailNodeIndex--;
        }
        CheckRailChangeTurretState(currentRailNodeIndex);

        reverseRail = true;
        thisMoveState = APCMoveState.STATE_RAIL;
    }

    // Sets the aim state to the last turret update from the current rail
    void CheckRailChangeTurretState(int index) {
        Rail.TurretUpdate lastUpdate = null;

        // Find the last turret state update
        if (reverseRail) {
            // Walk the nodes in reverse
            for (int i = currentRail.nodes.Length-1; i > index; i--) {
                foreach(var update in currentRail.turretUpdates) {
                    if (update.nodeIndex == i) // If there was a turret update for this node
                        lastUpdate = update;
                }
            }
        } else {
            for (int i = 0; i < index; i++) {
                foreach (var update in currentRail.turretUpdates) {
                    if (update.nodeIndex == i) // If there was a turret update for this node
                        lastUpdate = update;
                }
            }
        }

        if(lastUpdate != null) {
            thisAimState = APCAimState.STATE_LOCALDIRECTION;
            targetLocalTurretRotation = Quaternion.Euler(new Vector3(0, TurretDirectionToAngle(lastUpdate.direction), 0));
        }
    }

    public void SetTurretAimDir(TurretDirection dir) {
        thisAimState = APCAimState.STATE_LOCALDIRECTION;
        targetLocalTurretRotation = Quaternion.Euler(new Vector3(0, TurretDirectionToAngle(dir), 0));
    }

    // What this does is it rotates the truck 180 along the y axis, but keeps the turret rotation the same
    // This is usefull in stuff like looping rails to create the illusion of the truck just changing direction
    public void FlipDirection() {
        var currentTurretRot = turret.transform.rotation;
        transform.Rotate(Vector3.up, 180);
        turret.transform.rotation = currentTurretRot;

        currentVelocity = -currentVelocity;
    }
}

public enum APCState {
    STATE_SEARCHING,
    STATE_SPOTTED,
    STATE_CHASING
}

public enum APCMoveState {
    STATE_NONE,
    STATE_RAIL,
}

public enum APCAimState {
    STATE_LOCALDIRECTION, // Aim a specific local direction
    STATE_PLAYER         // Aim towards player
}

public enum TurretDirection {
    Forward,
    Backward,
    Left,
    Right,
}
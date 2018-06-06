using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour {

    public float ammo;
    public bool canShoot;
    public bool shooting;
    public bool reloading;

    public LineRenderer lr;
    public GameObject impactEffect;
    public AudioClip[] turretSFX;
    public GameObject shotFlash;

    public GameObject spotlight;
    Quaternion from, to;
    float timeToFace = 0f;

    public bool canSearch;
    private int currentTargetInSearch;
    private float currentTargetHoldTime;

    public delegate void DamageDelegate();
    public static event DamageDelegate HitPlayer;

    void Start() {
        spotlight = transform.GetChild(0).GetChild(0).gameObject;
        lr = GetComponent<LineRenderer>();
        shotFlash.SetActive(false);
    }

    public void Shoot()
    {
        if (ammo <= 0 && !reloading) {
            var reloadRoutine = Reload();
            StartCoroutine(reloadRoutine);
            return;
        }
        
        if (ammo > 0 && canShoot)
        {
            
            ammo--;

            GetComponent<AudioSource>().pitch = Random.Range(.6f, .8f);
            GetComponent<AudioSource>().PlayOneShot(turretSFX[0], .3f);

            // Check what we hit
            RaycastHit hit;

            var distance = Vector3.Distance(transform.position, transform.forward);

            var hitPoint = Vector3.zero;
            if (!Physics.Linecast(transform.position, transform.position + transform.forward * distance, out hit, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                hitPoint = transform.position + transform.forward * distance;
            }
            else
            {
                hitPoint = hit.point;
            }

            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hitPoint);
            StartCoroutine(AnimatingLineRendererShot(transform.position, hitPoint, 1, .2f));
            StartCoroutine(AnimatingShotFlash(.1f));

            if (hit.collider)
            {
                if (hit.transform.tag == "Player")
                {
                    print("hit player");
                    //HitPlayer();

                }
            }
            else if (hit.collider != null)
            {
                // Hit something other than the player, like the map
                GameObject impactObj = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactObj, 1.2f);

                print("hit something else");
            }

            var shootCooldownRoutine = ShootCooldown(Random.Range(.1f, .3f));
            StartCoroutine(shootCooldownRoutine);
            canShoot = false;
        }
    }

    IEnumerator ShootCooldown(float cooldown) {
        print("weapon recharging");
        yield return new WaitForSeconds(cooldown);
        canShoot = true;
        print("weapon recharged");
    }

    IEnumerator Reload()
    {
        GetComponent<AudioSource>().pitch = 1;
        GetComponent<AudioSource>().PlayOneShot(turretSFX[1], 1);
        print("reloading");
        reloading = true;
        yield return new WaitForSeconds(turretSFX[1].length);
        ammo = 20;
        canShoot = true;
        print("reloaded");
        reloading = false;
    }

    IEnumerator AnimatingLineRendererShot(Vector3 start, Vector3 end, float tracerLength, float tracerAirTime) {
        //lerp the first side of the lr it's length

        float lerpStartTime = Time.time;
        float lerpTimeSinceStarted = Time.time - lerpStartTime;
        float lerpPercentageComplete = lerpTimeSinceStarted / tracerAirTime;
        Vector3 ObjectStartPosition = start;
        
        while (true)
        {
            lr.SetPosition(0, start);
            lerpTimeSinceStarted = Time.time - lerpStartTime;
            lerpPercentageComplete = lerpTimeSinceStarted / tracerAirTime;
            print("Time since started: " + lerpTimeSinceStarted + " and Percent Complete" + lerpPercentageComplete);
            Vector3 currentPosition = Vector3.Lerp(start, end, lerpPercentageComplete);
            lr.SetPosition(1, currentPosition);
            if (lerpPercentageComplete >= tracerAirTime / tracerLength)
                lr.SetPosition(0, Vector3.Lerp(start, end, lerpPercentageComplete - tracerAirTime / tracerLength));
            if (lerpPercentageComplete >= 1) break;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator AnimatingShotFlash(float length) {
        shotFlash.SetActive(true);
        yield return new WaitForSeconds(length);
        shotFlash.SetActive(false);
    }

    void TurnOffTrail()
    {
        lr.SetPosition(1, transform.position);
    }

    public void FaceTarget(Vector3 desiredTargetPosition)
    {
        //Get current turret rotation
        Quaternion temp = transform.rotation;
        from = Quaternion.Euler(new Vector3(0, temp.eulerAngles.y, 0));

        //Get rotation based on desiredTarget position
        Vector3 relativePos = desiredTargetPosition - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(relativePos);

        //Assign desired Rotation without other axes
        to = Quaternion.Euler(new Vector3(0, desiredRotation.eulerAngles.y, 0));
        timeToFace = 0;
    }

    public void FaceDirection(Vector3 direction)
    {
        //Get current turret rotation
        Quaternion temp = transform.rotation;
        from = Quaternion.Euler(new Vector3(0, temp.eulerAngles.y, 0));

        //Assign desired Rotation without other axes
        to = Quaternion.Euler(new Vector3(0, direction.y, 0));
        timeToFace = 0;
    }


    public void SearchBetweenTargets(GameObject[] targetArray, float searchHoldTime) {
        //Get which target in the array is the closest to move to
        if (canSearch)
        {
            if (currentTargetInSearch < targetArray.Length - 1)
            {
                currentTargetInSearch++;
            }
            else if (currentTargetInSearch == targetArray.Length - 1)
            {
                currentTargetInSearch = 0;
            }
            StartCoroutine(SearchHold(searchHoldTime));
        }

        FaceTarget(targetArray[currentTargetInSearch].transform.position);
    }

    IEnumerator SearchHold(float holdTime) {
        print("Holding");
        canSearch = false;
        yield return new WaitForSeconds(holdTime);
        canSearch = true;
        print("Done holding");
    }

    public void RotationUpdate(float speed = 1)
    {
        //Apply turret changes
        timeToFace += Time.deltaTime * speed;

        //This is what allows for the rotation changes
        transform.rotation = Quaternion.Lerp(from, to, timeToFace);
    }

    //Slot this into a state machine, or the level script idc
    //it should run until the player is seen
    void TurretSearch(GameObject[] searchPositions)
    {
        if (searchPositions != null)
        {
            //Put all that jazz from below in here
        }
        else
        {
            print("Turret can't search, dummy");
        }

        //Maybe an entrance behavior below
        //If not narrow spotlight
        //Narrow Turret Spotlight

        //Pass search positions to the function, x time to wait on each position, and y speed of turret rotation
        //Start with position 0
        //For each position
        //If not facing that position
        //rotate to face it
        //return until facing
        //If facing that position
        //If x seconds haven't elapsed
        //Wait for x seconds
        //return
        //If x seconds have elapsed
        //if at end of array
        //reset to start of array / 0
        //if not at end of array
        //continue
    }
}

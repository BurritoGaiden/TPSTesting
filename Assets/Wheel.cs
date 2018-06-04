using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour {

    public void Spin(bool forward, float speed) {
        //transform.Rotate(Vector3.forward * speed * Time.deltaTime);

        //Vector3 relativePos = transform.position + new Vector3(0, 0, 0);
        int forwards = forward == true ? -1 : 1;
        transform.RotateAround(transform.position, Vector3.right, forwards * speed * Time.deltaTime);
    }

    public void TurnWheels() {
        transform.Rotate(Vector3.forward, 8);
    }

    public void InitiateTurn(Vector3 v, float time) {
        StartCoroutine(Turn(v, time));
    }

    public IEnumerator Turn(Vector3 desiredRotation, float timeToLerp)
    {
        float lerpStartTime = Time.time;
        float lerpTimeSinceStarted = Time.time - lerpStartTime;
        float lerpPercentageComplete = lerpTimeSinceStarted / timeToLerp;

        Vector3 ObjectStartRotation = transform.rotation.eulerAngles;

        while (true)
        {
            lerpTimeSinceStarted = Time.time - lerpStartTime;
            lerpPercentageComplete = lerpTimeSinceStarted / timeToLerp;

            print("Time since started: " + lerpTimeSinceStarted + " and Percent Complete" + lerpPercentageComplete);

            Vector3 currentRotation = Vector3.Lerp(ObjectStartRotation, desiredRotation, lerpPercentageComplete);
            transform.rotation.SetEulerRotation(currentRotation);

            if (lerpPercentageComplete >= 1) break;
            yield return new WaitForEndOfFrame();
        }

        print("Done with Lerping tires");
    }

   
}

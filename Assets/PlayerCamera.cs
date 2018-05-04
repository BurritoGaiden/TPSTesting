using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    //NOTE: THIS SCRIPT IS ON THE CAMERA RIG AT THIS TIME
    public Transform cam;
    public bool camInput;
    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Transform target;
    public float dstFromTarget = 2;
    public Vector2 pitchMinMax = new Vector2(-30, 60);

    public float rotationSmoothTime = .12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;
    
    float yaw;
    float pitch;

    //testing
    public Vector2 testYP;
    public static Transform camTar;


    public static camStates cameraState = camStates.STATE_NULL;

    //Sample received delegate string
    public string lookingFor;

    //FOR DEBUGGING PURPOSES
    public string currentStateString;

    public float horizontalOffset = .64f;
    public float forwardOffset = 0f;
    public float verticalOffset = 0f;

    void Awake() {
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        cam = Camera.main.transform;

        LevelScript.ECamInput += EnableCameraInput;
        LevelScript.DCamInput += DisableCameraInput;
        LevelScript.SetCharCamTransform += SetCameraTransform;
        LevelScript.ResetCamPositionOnRig += ResetCameraOnRig;
    }
   
	void LateUpdate () {

        switch (cameraState)
        {
            //State Case
            case camStates.STATE_PLAYERORBIT:
                //State Behavior
                OrbitingBehavior();
                CameraOffset();
                //State Transitions
                if (!camInput) cameraState = camStates.STATE_NULL;
                else if (PlayerController.thisMoveState == MoveState.STATE_COVER) cameraState = camStates.STATE_COVER;
                else if (Interesting.looking) cameraState = camStates.STATE_POIFOCUS;
                else if (Killing.aiming) cameraState = camStates.STATE_PLAYERAIM;
                break;

                //Developer driven state. Can only be switched into and out of from the level script
            case camStates.STATE_DIRFOCUS:
                FocusBehavior();
                CameraOffset();
                break;
            case camStates.STATE_COVER:
                OrbitingBehavior();
                CameraOffset();
                if (PlayerController.thisMoveState != MoveState.STATE_COVER) cameraState = camStates.STATE_PLAYERORBIT;
                break;
            case camStates.STATE_POIFOCUS:
                FocusBehavior();
                CameraOffset();
                if (!Interesting.looking) cameraState = camStates.STATE_PLAYERORBIT;
                break;

            case camStates.STATE_NULL:
                if (camInput) cameraState = camStates.STATE_PLAYERORBIT;
                break;

            case camStates.STATE_PLAYERAIM:
                AimingBehavior();

                if (!Killing.aiming) cameraState = camStates.STATE_PLAYERORBIT;
                break;
        }
	}

    void OrbitingBehavior() {
        //Mouse control of pitch and yaw
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //Applying control to camera rotation in a smoothed fashion
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * dstFromTarget;  
    }

    void AimingBehavior()
    {
        //Mouse control of pitch and yaw
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity / 3;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity / 3;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //Applying control to camera rotation in a smoothed fashion
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * dstFromTarget;
    }

    void FocusBehavior()
    {
        yaw = GetAngleBetween3PointsHor(this.transform.position, camTar.position);
        pitch = GetAngleBetween3PointsVer(this.transform.position, camTar.position);
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * dstFromTarget;        
    }

    float GetAngleBetween3PointsHor(Vector3 a, Vector3 b)
    {
        float theta = Mathf.Atan2(b.x - a.x, b.z - a.z);
        float angle = theta * 180 / Mathf.PI;
        return angle;
    }

    float GetAngleBetween3PointsVer(Vector3 a, Vector3 b)
    {
        var dist = Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

        float theta = Mathf.Atan2(b.y - a.y, dist);
        float angle = theta * -180 / Mathf.PI;
        return angle;
    }


    void CameraOffset() {
        //Adding Camera offset
        
        if (Killing.aiming) {
            forwardOffset = 1f;
            horizontalOffset = .64f;
            verticalOffset = 0f;
        }
        if (PlayerController.thisMoveState == MoveState.STATE_REGULAR) {
            //forwardOffset -= PlayerController.currentSpeed / 3;
            forwardOffset = .66f;
            horizontalOffset = .73f;
            if (!PlayerController.crouchInput)
                verticalOffset = .36f;
            else
                verticalOffset = -.15f;
        }
        if (PlayerController.thisMoveState == MoveState.STATE_COVER) {
            verticalOffset = -.15f;
            forwardOffset = .66f;
            horizontalOffset = 0f;
        }
        Vector3 camOffset = new Vector3(horizontalOffset, verticalOffset, forwardOffset);
        cam.transform.localPosition = camOffset;
    }

    float CameraDSTFromTarget() {
        //declare temp float
        float DST;
        //if the pitch is at the pitch min, the distance will be the closest
        if (pitch == pitchMinMax.x) DST = 1;
        //if the pitch is below regular range, lerp to
        else if (pitch <= 10) DST = Mathf.Lerp(1, 3, (pitch + 30) / 50);
        //if the pitch is below high range, lerp to
        else if (pitch <= 30) DST = 3;
        else DST = Mathf.Lerp(3, 5.5f, (pitch - 51) / 20);
        return DST;
    }

    void SetCameraTransform(Vector3 position, Vector3 rotation)
    {
        Camera.main.transform.localPosition = position;
        Camera.main.transform.localRotation = Quaternion.Euler(rotation);
    }

    void EnableCameraInput() {
        camInput = true;
    }

    void DisableCameraInput() {
        camInput = false;
    }

    void ResetCameraOnRig() {
        Camera.main.transform.position = this.transform.position;
        Camera.main.transform.rotation = this.transform.rotation;
    }  
}

public enum camStates
{
    STATE_PLAYERORBIT,
    STATE_DIRFOCUS,
    STATE_POIFOCUS,
    STATE_NULL,
    STATE_COVER,
    STATE_COVERAIM,
    STATE_PLAYERAIM
};

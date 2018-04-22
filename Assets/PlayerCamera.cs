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
    public Transform camTar;

    //vars that are from other scripts not made yet
    public bool aiming;
    public bool running;
    public bool looking;

    enum camStates {
        STATE_PLAYERORBIT,
        STATE_POIFOCUS
    };
    camStates cameraState;

    //Sample received delegate string
    public string lookingFor;

    void Awake() {
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        cam = transform.GetChild(0);

        cameraState = camStates.STATE_PLAYERORBIT;

        LevelScript.ECamInput += EnableCameraInput;
        LevelScript.DCamInput += DisableCameraInput;
        LevelScript.SetCharCamTransform += SetCameraTransform;
        LevelScript.ResetCamPositionOnRig += ResetCameraOnRig;
    }

    void Update() {
        CheckInputs();
    }

    void CheckInputs() {
        running = Input.GetKey(KeyCode.LeftShift);
        aiming = Input.GetKey(KeyCode.Mouse1);
        if(Interesting.canLook)looking = Input.GetKey(KeyCode.E);
    }
    
	void LateUpdate () {
        if (looking)
        {
            cameraState = camStates.STATE_POIFOCUS;
        }
        else {
            cameraState = camStates.STATE_PLAYERORBIT;
        }

        switch (cameraState)
        {
            case camStates.STATE_PLAYERORBIT:
                if (camInput) OrbitingBehavior();
                return;
            case camStates.STATE_POIFOCUS:
                FocusBehavior();
                return;
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

        transform.position = target.position - transform.forward * CameraDSTFromTarget();

        CameraOffset();
    }

    void FocusBehavior()
    {
        yaw = GetAngleBetween3PointsHor(this.transform.position, camTar.position);
        pitch = GetAngleBetween3PointsVer(this.transform.position, camTar.position);
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * CameraDSTFromTarget();

        CameraOffset();
    }

    float GetAngleBetween3PointsHor(Vector3 a, Vector3 b)
    {
        float theta = Mathf.Atan2(b.x - a.x, b.z - a.z);
        float angle = theta * 180 / Mathf.PI;
        return angle;
    }

    float GetAngleBetween3PointsVer(Vector3 a, Vector3 b)
    {
        float theta = Mathf.Atan2(b.y - a.y, b.z - a.z);
        float angle = theta * -180 / Mathf.PI;
        return angle;
    }
   
    void CameraOffset() {
        //Adding Camera offset
        float regularWalkOffset = .64f;
        float aimOffset = 0f;
        if (aiming) {
            aimOffset += 1f;
        }
        if (running) {
            aimOffset -= 1f;
        }

        Vector3 camOffset = new Vector3(regularWalkOffset, 0, aimOffset);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    /*
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
    public bool aimInput;
    public bool runInput;
    public bool lookInput;
    public bool coverInput;

    float regularWalkOffset = .64f;
    float camHorizontalOffset = 0f;
    float camForwardOffset = 0f;

    enum camStates
    {
        STATE_PLAYERORBIT,
        STATE_POIFOCUS,
        STATE_AIMING,
        STATE_COVER,
        STATE_RUNNING
    };
    camStates cameraState;

    //Sample received delegate string
    public string lookingFor;

    void Awake()
    {
        if (lockCursor)
        {
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

    void Update()
    {
        CheckInputs();
        SetRigState();
        CameraRigStateBehaviors();
        CameraOffset();
    }
    #region loop
    void CheckInputs()
    {
        runInput = Input.GetKey(KeyCode.LeftShift);
        aimInput = Input.GetKey(KeyCode.Mouse1);
        coverInput = Input.GetKey(KeyCode.K);
        if (Interesting.canLook) lookInput = Input.GetKey(KeyCode.E);
    }

    void SetRigState()
    {
        if (camInput)
        {
            if (lookInput) cameraState = camStates.STATE_POIFOCUS;
            else if (runInput) cameraState = camStates.STATE_RUNNING;
            else if (aimInput) cameraState = camStates.STATE_AIMING;
            else if (coverInput) cameraState = camStates.STATE_COVER;
            else cameraState = camStates.STATE_PLAYERORBIT;
        }
        else cameraState = camStates.STATE_PLAYERORBIT;
    }

    void CameraRigStateBehaviors()
    {
        switch (cameraState)
        {
            case camStates.STATE_PLAYERORBIT:
                OrbitingBehavior();
                break;
            case camStates.STATE_AIMING:
                AimingBehavior();
                break;
            case camStates.STATE_POIFOCUS:
                FocusBehavior();
                break;
            case camStates.STATE_RUNNING:
                OrbitingBehavior();
                break;
            case camStates.STATE_COVER:
                OrbitingBehavior();
                break;
        }
    }
    #endregion

    #region behaviors
    void OrbitingBehavior()
    {

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

    void AimingBehavior()
    {

        //Mouse control of pitch and yaw
        yaw += Input.GetAxis("Mouse X") * (mouseSensitivity / 3);
        pitch -= Input.GetAxis("Mouse Y") * (mouseSensitivity / 3);
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //Applying control to camera rotation in a smoothed fashion
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * dstFromTarget;

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
    #endregion

    #region helpers
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
    #endregion

    #region Camera Behaviors
    void CameraOffset()
    {
        if (cameraState == camStates.STATE_AIMING)
        {
            camHorizontalOffset += .70f;
            camForwardOffset += 1f;
        }
        else if (cameraState == camStates.STATE_RUNNING)
        {
            camForwardOffset -= 1f;
            camHorizontalOffset += 0;
        }
        else if (cameraState == camStates.STATE_PLAYERORBIT)
        {
            camForwardOffset -= 0;
            camHorizontalOffset += .64f;
        }
        else if (cameraState == camStates.STATE_POIFOCUS)
        {
            camForwardOffset += 1f;
            camHorizontalOffset += .70f;
        }

        cam.transform.localPosition = new Vector3(regularWalkOffset, 0, camForwardOffset);
    }

    float CameraDSTFromTarget()
    {
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
    #endregion

    #region static-ey callback functions
    void SetCameraTransform(Vector3 position, Vector3 rotation)
    {
        Camera.main.transform.localPosition = position;
        Camera.main.transform.localRotation = Quaternion.Euler(rotation);
    }

    void EnableCameraInput()
    {
        camInput = true;
    }

    void DisableCameraInput()
    {
        camInput = false;
    }

    void ResetCameraOnRig()
    {
        Camera.main.transform.position = this.transform.position;
        Camera.main.transform.rotation = this.transform.rotation;
    }
    #endregion
    */
}

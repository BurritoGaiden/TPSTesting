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
    float rotationSmoothVelocityX, rotationSmoothVelocityY;
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

    // For use with the RAIL state
    public static Transform followRail;
    public static float railOffset;

    public static bool setRotationInstantlyNextFrame;

    // Used to smoothly transition between different states like the rail state one
    public static bool transitioning;

    public static Vector3 detachedPosition;
    public static Quaternion detachedFixedRotation = Quaternion.identity;

    public LayerMask occlusionLayers = Physics.DefaultRaycastLayers;

    public static string playingAnim;
    Animator anim;

    void Awake() {
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        cam = Camera.main.transform;

        LevelScript.EnableCameraInput += EnableCameraInput;
        LevelScript.DisableCameraInput += DisableCameraInput;
        LevelScript.SetCharCamTransform += SetCameraTransform;
        LevelScript.ResetCamPositionOnRig += ResetCameraOnRig;
        LevelScript.EnableDirectorFocus += EnDirFocus;
        LevelScript.DisableDirectorFocus += DisDirFocus;

        anim = GetComponent<Animator>();
    }

    void EnDirFocus() {
        cameraState = camStates.STATE_DIRFOCUS;
    }

    void DisDirFocus() {
        cameraState = camStates.STATE_PLAYERORBIT;
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
                break;

                //Developer driven state. Can only be switched into and out of from the level script
            case camStates.STATE_DIRFOCUS:
                FocusBehavior();
                CameraOffset();
                break;
            case camStates.STATE_RAIL:
                FocusBehavior();

                var pos = followRail.position + Vector3.Project(target.transform.position - followRail.position, followRail.forward);
                transform.position = pos + (railOffset * followRail.forward);
                break;
            case camStates.STATE_COVER:
                OrbitingBehavior();
                CameraOffset();
                if (PlayerController.thisMoveState != MoveState.STATE_COVER) cameraState = camStates.STATE_PLAYERORBIT;
                break;
            case camStates.STATE_PUSHING:
                OrbitingBehavior();
                CameraOffset();
                if (PlayerController.thisMoveState != MoveState.STATE_PUSHING) cameraState = camStates.STATE_PLAYERORBIT;
                break;
            case camStates.STATE_POIFOCUS:
                FocusBehavior();
                CameraOffset();
                if (!Interesting.looking) cameraState = camStates.STATE_PLAYERORBIT;
                break;

            case camStates.STATE_NULL:
                if (camInput) cameraState = camStates.STATE_PLAYERORBIT;
                break;

            case camStates.STATE_DETACHED:
                var savedPos = transform.position;
                if (Quaternion.identity == detachedFixedRotation) {
                    FocusBehavior();
                } else {
                    transform.rotation = Quaternion.Lerp(transform.rotation, detachedFixedRotation, Time.deltaTime * 5);
                }

                Debug.Log("Detached boi");
                transform.position = Vector3.MoveTowards(savedPos, detachedPosition, Time.deltaTime * 15);

                break;
            case camStates.STATE_PLAYINGANIM:
                //transform.position = target.position;
                //transform.rotation = Quaternion.identity;
                CameraOffset();

                if (!anim.GetCurrentAnimatorStateInfo(0).IsName(playingAnim)) {
                    cameraState = camStates.STATE_PLAYERORBIT;
                } else {
                    yaw = transform.rotation.eulerAngles.y;
                    pitch = transform.rotation.eulerAngles.x;
                    currentRotation = new Vector3(pitch, yaw);
                    Debug.Log(yaw);
                }

                break;
        }
	}

    public static void PlayAnim(string a) {
        var camAnim = Camera.main.transform.parent.GetComponent<Animator>();
        camAnim.Play(a, 0);
        cameraState = camStates.STATE_PLAYINGANIM;
        playingAnim = a;
    }

    void OrbitingBehavior()
    {
        //Mouse control of pitch and yaw
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //Applying control to camera rotation in a smoothed fashion
        currentRotation.x = Mathf.SmoothDampAngle(currentRotation.x, pitch, ref rotationSmoothVelocityX, rotationSmoothTime);
        currentRotation.y = Mathf.SmoothDampAngle(currentRotation.y, yaw, ref rotationSmoothVelocityY, rotationSmoothTime);
        //currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        UpdatePosition();
    }

    void UpdatePosition()
    {
        var targetPosition = target.position - transform.forward * dstFromTarget;
        if (!transitioning || Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transitioning = false;
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 10);
        }
    }

    void AimingBehavior()
    {
        //Mouse control of pitch and yaw
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity / 3;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity / 3;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //Applying control to camera rotation in a smoothed fashion
        currentRotation.x = Mathf.SmoothDampAngle(currentRotation.x, pitch, ref rotationSmoothVelocityX, rotationSmoothTime);
        currentRotation.y = Mathf.SmoothDampAngle(currentRotation.y, yaw, ref rotationSmoothVelocityY, rotationSmoothTime);
        //currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * dstFromTarget;
    }

    void FocusBehavior()
    {
        yaw = GetAngleBetween3PointsHor(this.transform.position, camTar.position);
        pitch = GetAngleBetween3PointsVer(this.transform.position, camTar.position);
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        if (setRotationInstantlyNextFrame)
        {
            currentRotation.x = pitch;
            currentRotation.y = yaw;
            setRotationInstantlyNextFrame = false;
        }
        else
        {
            currentRotation.x = Mathf.SmoothDampAngle(currentRotation.x, pitch, ref rotationSmoothVelocityX, rotationSmoothTime);
            currentRotation.y = Mathf.SmoothDampAngle(currentRotation.y, yaw, ref rotationSmoothVelocityY, rotationSmoothTime);
            //currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        }

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
        
        // If an animation is being played, then we wanna stay at origin
        if (cameraState == camStates.STATE_PLAYINGANIM) {
            cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, Vector3.zero, Time.deltaTime * 10);
            return;
        }

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
        if (PlayerController.thisMoveState == MoveState.STATE_PUSHING) {
            verticalOffset = 0;
            forwardOffset = .66f;
            horizontalOffset = -1f;
        }

        Vector3 camOffset = new Vector3(horizontalOffset, verticalOffset, forwardOffset);

        float distToKeepFromWall = 0.35f;

        // make sure there are no walls between the camera and the player
        // this dosnt catch the far away cases, which is why we also have a secondary check after this
        RaycastHit hit;
        if (Physics.Linecast(target.position, transform.TransformPoint(camOffset), out hit, occlusionLayers)) {
            Debug.DrawLine(target.position, transform.TransformPoint(camOffset));
            camOffset = transform.InverseTransformPoint(hit.point + hit.normal * distToKeepFromWall); // Keep a small distance from the wall so that we cant see through it
        }

        var dirs = new Vector3[] {
            Vector3.right,
            Vector3.left,
            Vector3.forward,
            Vector3.back,
        };

        // Secondary check wich casts a ray out from the camera in call directions
        foreach(var dir in dirs) {
            // Raycast to each side
            var camPosWithOffset = transform.TransformPoint(camOffset);

            if (Physics.Linecast(camPosWithOffset, camPosWithOffset + dir* distToKeepFromWall, out hit, occlusionLayers)) {
                var dirAway = camPosWithOffset - hit.point;
                camOffset = transform.InverseTransformPoint(hit.point + hit.normal * distToKeepFromWall);
            }
        }

        cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, camOffset, Time.deltaTime*5);
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
    STATE_PUSHING,
    STATE_COVERAIM,
    STATE_PLAYERAIM,
    STATE_RAIL,
    STATE_DETACHED,
    STATE_PLAYINGANIM,
};

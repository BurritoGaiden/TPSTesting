using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    //NOTE: THIS SCRIPT IS ON THE CAMERA RIG AT THIS TIME
    public Transform cam;
    public bool camInput;
    public float mouseSensitivity = 10;
    public Transform orbitTarget;
    public Transform target;
    public float dstFromTarget = 2;
    public Vector2 pitchMinMax = new Vector2(-30, 60);

    public float rotationSmoothTime = .12f;
    Vector3 rotationSmoothVelocity;
    float rotationSmoothVelocityX, rotationSmoothVelocityY;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    public static Transform cameraLookTarget;
    public static Transform targetPosition;
    public static Transform valuePlaceholder;

    public static camStates cameraState = camStates.STATE_NULL;
    public static cameraStates camState = cameraStates.STATE_NOTHING;

    public float horizontalOffset = .64f;
    public float forwardOffset = 0f;
    public float verticalOffset = 0f;

    public static bool setRotationInstantlyNextFrame;

    // Used to smoothly transition between different states like the rail state one
    public static bool transitioning;

    public static Vector3 detachedPosition;
    public static Quaternion detachedFixedRotation = Quaternion.identity;

    public LayerMask occlusionLayers = Physics.DefaultRaycastLayers;

    public static string playingAnim;
    Animator anim;

    public Transform desiredView;
    public static float transitionSpeed;
    public static Transform currentView;
    public bool lerpToPos;
    public GameObject rigVisualMarker;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = transform.GetChild(0);

        LevelScript.b_SetCameraInput += SetInput;
        LevelScript.ResetCamPositionOnRig += ResetCameraOnRig;
        LevelScript.st_SetCameraState += SetCameraState;
    }

    void SetInput(bool desiredBool) { camInput = desiredBool; }

    void LateUpdate()
    {
        CameraStateLogic();
    }
    void CameraCover()
    {
        OrbitingBehavior();
        UpdatePosition();
        CameraOffset();
        if (PlayerController.thisMoveState != MoveState.STATE_COVER) cameraState = camStates.STATE_PLAYERORBIT;
    }
    void PuzzleDirFocus()
    {
        //Set up placeholder to take data
        valuePlaceholder = transform;
        //Establishing rotation
        yaw = GetAngleBetween3PointsHor(this.transform.position, cameraLookTarget.position);
        pitch = GetAngleBetween3PointsVer(this.transform.position, cameraLookTarget.position);
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
        }

        //---------------------------------------------------

        //Assign rotation values to placeholder that'll make the angle lerp work
        valuePlaceholder.eulerAngles = currentRotation;

        //---------------------------------------------------

        //Lerping to a position and rotation
        transform.position = Vector3.Lerp(transform.position, targetPosition.position, Time.deltaTime * transitionSpeed);
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, CamOffset(), Time.deltaTime * transitionSpeed);

        //Turn the rotation we got from the rot/pos establishment and set it in the current angle
        Vector3 currentAnglePL = new Vector3(
            Mathf.LerpAngle(transform.rotation.eulerAngles.x, valuePlaceholder.rotation.eulerAngles.x, Time.deltaTime * transitionSpeed),
            Mathf.LerpAngle(transform.rotation.eulerAngles.y, valuePlaceholder.rotation.eulerAngles.y, Time.deltaTime * transitionSpeed),
            Mathf.LerpAngle(transform.rotation.eulerAngles.z, valuePlaceholder.rotation.eulerAngles.z, Time.deltaTime * transitionSpeed));

        //Assign to our rig :)
        transform.eulerAngles = currentAnglePL;
    }
    void LerpDirFocus()
    {
        currentView = transform;
        //Establishing position and rotation
        yaw = GetAngleBetween3PointsHor(this.transform.position, cameraLookTarget.position);
        pitch = GetAngleBetween3PointsVer(this.transform.position, cameraLookTarget.position);
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

        //---------------------------------------------------

        //Assign rotation values to placeholder that'll make the angle lerp work
        currentView.eulerAngles = currentRotation;

        //---------------------------------------------------

        //Lerping to a position and rotation
        transform.position = LerpOrMoveTowardsPoisition(transform.position, target.position - transform.forward * dstFromTarget, transitionSpeed, transitionSpeed / 3f);
        cam.transform.localPosition = LerpOrMoveTowardsPoisition(cam.transform.localPosition, CamOffset(), transitionSpeed, transitionSpeed / 3f);

        //Turn the rotation we got from the rot/pos establishment and set it in the current angle
        Vector3 currentAngle = new Vector3(
            Mathf.LerpAngle(transform.rotation.eulerAngles.x, currentView.transform.rotation.eulerAngles.x, Time.deltaTime * transitionSpeed),
            Mathf.LerpAngle(transform.rotation.eulerAngles.y, currentView.transform.rotation.eulerAngles.y, Time.deltaTime * transitionSpeed),
            Mathf.LerpAngle(transform.rotation.eulerAngles.z, currentView.transform.rotation.eulerAngles.z, Time.deltaTime * transitionSpeed));

        //Assign to our rig :)
        transform.eulerAngles = currentAngle;
    }
    void CameraStateLogic()
    {
        switch (camState)
        {
            //State Case
            case cameraStates.d_PlayerRigOrbit:
                PlayerControlledRigOrbit(orbitTarget.position);
                break;
            case cameraStates.d_PlayerRigOrbit_UpdatePosition:
                PlayerControlledRigOrbit(orbitTarget.position);
                //CameraLookAt(GameObject.Find("part1Trigger01"));

                break;

            case cameraStates.STATE_PLAYERORBIT:
                
                OrbitingBehavior();
                UpdatePosition();
                CameraOffset();

                //State Transitions
                if (!camInput) cameraState = camStates.STATE_NULL;
                else if (PlayerController.thisMoveState == MoveState.STATE_COVER) cameraState = camStates.STATE_COVER;
                else if (Interesting.looking) cameraState = camStates.STATE_POIFOCUS;
                break;

            case cameraStates.STATE_ROTATETOLOOKAT:
                //CCTVBehavior();
                //CCTVPlayerBehavior();
                OrbitingBehavior();
                LookAtBehavior(cam.gameObject);
                //UpdatePosition();

                break;


            case cameraStates.STATE_POIFOCUS:
                FocusBehavior();
                CameraOffset();
                if (!Interesting.looking) cameraState = camStates.STATE_PLAYERORBIT;
                break;

            case cameraStates.STATE_COVER:
                CameraCover();
                break;

            //--------------------------------------------

            //Developer driven state. Can only be switched into and out of from the level script
            case cameraStates.STATE_DIRFOCUS:
                FocusBehavior();
                CameraOffset();

                break;

            
            
            case cameraStates.STATE_LERPDIRFOCUS:
                LerpDirFocus();
                break;

            //Needs to lerp away from regular player cam position to a new position
            //Needs to also track the player position with it's rotation
            case cameraStates.STATE_PUZZLELERPDIRFOCUS:
                PuzzleDirFocus();
                break;

            case cameraStates.STATE_LERPING:
                transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);

                Vector3 currentAngleL = new Vector3(
                    Mathf.LerpAngle(transform.rotation.eulerAngles.x, currentView.transform.rotation.eulerAngles.x, Time.deltaTime * transitionSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.y, currentView.transform.rotation.eulerAngles.y, Time.deltaTime * transitionSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.z, currentView.transform.rotation.eulerAngles.z, Time.deltaTime * transitionSpeed));

                transform.eulerAngles = currentAngleL;
                CameraOffset();
                break;

            case cameraStates.STATE_PUZZLELERPING:
                transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);

                Vector3 currentAngleP = new Vector3(
                    Mathf.LerpAngle(transform.rotation.eulerAngles.x, currentView.transform.rotation.eulerAngles.x, Time.deltaTime * transitionSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.y, currentView.transform.rotation.eulerAngles.y, Time.deltaTime * transitionSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.z, currentView.transform.rotation.eulerAngles.z, Time.deltaTime * transitionSpeed));

                transform.eulerAngles = currentAngleP;
                CameraOffset();
                break;

            case cameraStates.STATE_JUSTORBIT:
                OrbitingBehavior();

                break;

            case cameraStates.STATE_PUZZLECCTV:
                yaw = GetAngleBetween3PointsHor(this.transform.position, cameraLookTarget.position);
                pitch = GetAngleBetween3PointsVer(this.transform.position, cameraLookTarget.position);
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
                CameraOffset();
                break;

            case cameraStates.STATE_DETACHED:
                var savedPos = transform.position;
                if (Quaternion.identity == detachedFixedRotation)
                {
                    FocusBehavior();
                }
                else
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, detachedFixedRotation, Time.deltaTime * 5);
                }

                //Debug.Log("Detached boi");
                transform.position = Vector3.MoveTowards(savedPos, detachedPosition, Time.deltaTime * 15);

                break;
        }
    }

    void CCTVPlayerBehavior()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        Vector3 targetRotation = new Vector3(pitch, yaw);
        transform.eulerAngles = targetRotation;
    }

    void CCTVBehavior()
    {
        yaw = GetAngleBetween3PointsHor(this.transform.position, cameraLookTarget.position);
        pitch = GetAngleBetween3PointsVer(this.transform.position, cameraLookTarget.position);
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
    }

    void PlayerControlledRigRotate(Vector3 pointToOrbit)
    {
        //Mouse control of pitch and yaw
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //Applying control to rig rotation in a smoothed fashion
        currentRotation.x = Mathf.SmoothDampAngle(currentRotation.x, pitch, ref rotationSmoothVelocityX, rotationSmoothTime);
        currentRotation.y = Mathf.SmoothDampAngle(currentRotation.y, yaw, ref rotationSmoothVelocityY, rotationSmoothTime);

        //Set the Rotation
        transform.eulerAngles = currentRotation;
    }

    void PlayerControlledRigOrbit(Vector3 pointToOrbit)
    {
        //Mouse control of pitch and yaw
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //Applying control to rig rotation in a smoothed fashion
        currentRotation.x = Mathf.SmoothDampAngle(currentRotation.x, pitch, ref rotationSmoothVelocityX, rotationSmoothTime);
        currentRotation.y = Mathf.SmoothDampAngle(currentRotation.y, yaw, ref rotationSmoothVelocityY, rotationSmoothTime);

        //Set the Rotation
        transform.eulerAngles = currentRotation;

        //Set the position we'd like the camera to be at as dst from Orbit point, relative to the forward position of the Camera
        var targetPosition = pointToOrbit - transform.forward * dstFromTarget;
        if (!transitioning || Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transitioning = false;
            //rigVisualmarker.transform.position = targetPosition;
            transform.position = targetPosition;
        }
        else
        {
            //rigVisualmarker.transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 10);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 10);
        }
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

    public void CameraLookAt(GameObject lookTarget)
    {
        yaw = GetAngleBetween3PointsHor(cam.transform.position, lookTarget.transform.position);
        pitch = GetAngleBetween3PointsVer(cam.transform.position, lookTarget.transform.position);
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        Vector3 currentCameraRotation = cam.transform.rotation.eulerAngles;

        if (setRotationInstantlyNextFrame)
        {
            currentCameraRotation.x = pitch;
            currentCameraRotation.y = yaw;
            setRotationInstantlyNextFrame = false;
        }
        else
        {
            currentCameraRotation.x = Mathf.SmoothDampAngle(currentCameraRotation.x, pitch, ref rotationSmoothVelocityX, rotationSmoothTime);
            currentCameraRotation.y = Mathf.SmoothDampAngle(currentCameraRotation.y, yaw, ref rotationSmoothVelocityY, rotationSmoothTime);
            //currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        }

        cam.transform.eulerAngles = currentCameraRotation;
    }

    public void LookAtBehavior(GameObject whateverObject)
    {
        yaw = GetAngleBetween3PointsHor(whateverObject.transform.position, cameraLookTarget.position);
        pitch = GetAngleBetween3PointsVer(whateverObject.transform.position, cameraLookTarget.position);
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

        whateverObject.transform.eulerAngles = currentRotation;
    }

        void FocusBehavior()
    {
        yaw = GetAngleBetween3PointsHor(this.transform.position, cameraLookTarget.position);
        pitch = GetAngleBetween3PointsVer(this.transform.position, cameraLookTarget.position);
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

    Vector3 CamOffset()
    {
        //Adding Camera offset
        if (CharacterUserControl.aimInput)
        {
            forwardOffset = 1f;
            horizontalOffset = .64f;
            verticalOffset = 0f;
        }
        if (PlayerController.thisMoveState == MoveState.STATE_REGULAR || PlayerController.thisMoveState == MoveState.STATE_SCRIPTEDMOVEMENT)
        {
            //forwardOffset -= PlayerController.currentSpeed / 3;
            forwardOffset = 0f;
            horizontalOffset = .73f;
            if (!CharacterUserControl.crouchInput)
                verticalOffset = .36f;
            else
                verticalOffset = -.15f;
        }
        if (PlayerController.thisMoveState == MoveState.STATE_COVER)
        {
            verticalOffset = -.15f;
            forwardOffset = .66f;
            horizontalOffset = 0f;
        }

        Vector3 camOffset = new Vector3(horizontalOffset, verticalOffset, forwardOffset);
        return camOffset;
    }

    void CameraOffset()
    {
        //Adding Camera offset
        if (cameraState == camStates.STATE_PUZZLECCTV || cameraState == camStates.STATE_PUZZLELERPING)
        {
            //forwardOffset -= PlayerController.currentSpeed / 3;
            forwardOffset = .66f;
            horizontalOffset = .73f;
            if (!CharacterUserControl.crouchInput)
                verticalOffset = .36f;
            else
                verticalOffset = -.15f;

        }
        if (CharacterUserControl.aimInput)
        {
            forwardOffset = 1f;
            horizontalOffset = .64f;
            verticalOffset = 0f;
        }
        if (PlayerController.thisMoveState == MoveState.STATE_REGULAR)
        {
            //forwardOffset -= PlayerController.currentSpeed / 3;
            forwardOffset = 0f;
            horizontalOffset = .73f;
            if (!CharacterUserControl.crouchInput)
                verticalOffset = .36f;
            else
                verticalOffset = -.15f;
        }
        if (PlayerController.thisMoveState == MoveState.STATE_COVER)
        {
            verticalOffset = -.15f;
            forwardOffset = .66f;
            horizontalOffset = 0f;
        }

        Vector3 camOffset = new Vector3(horizontalOffset, verticalOffset, forwardOffset);

        float distToKeepFromWall = 0.35f;

        // make sure there are no walls between the camera and the player
        // this dosnt catch the far away cases, which is why we also have a secondary check after this
        RaycastHit hit;
        if (Physics.Linecast(target.position, transform.TransformPoint(camOffset), out hit, occlusionLayers))
        {
            Debug.DrawLine(target.position, transform.TransformPoint(camOffset));
            camOffset = transform.InverseTransformPoint(hit.point + hit.normal * distToKeepFromWall); // Keep a small distance from the wall so that we cant see through it
        }

        var dirs = new Vector3[] {
            Vector3.right,
            Vector3.left,
            Vector3.forward,
            Vector3.back,
        };

        // Secondary check which casts a ray out from the camera in call directions
        foreach (var dir in dirs)
        {
            // Raycast to each side
            var camPosWithOffset = transform.TransformPoint(camOffset);

            if (Physics.Linecast(camPosWithOffset, camPosWithOffset + dir * distToKeepFromWall, out hit, occlusionLayers))
            {
                var dirAway = camPosWithOffset - hit.point;
                camOffset = transform.InverseTransformPoint(hit.point + hit.normal * distToKeepFromWall);
            }
        }
        cam.transform.localPosition = CamOffset();
        //cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, camOffset, Time.deltaTime * 5);
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

    // Either lerps towards position, or if the distance lerped is lower than lowerSpeed, then uses Vector3.MoveTowards
    Vector3 LerpOrMoveTowardsPoisition(Vector3 currentPos, Vector3 targetPos, float lerpSpeed, float lowestSpeed)
    {
        var newPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * lerpSpeed);
        if (Vector3.Distance(newPos, currentPos) < Time.deltaTime * lowestSpeed)
        {
            newPos = Vector3.MoveTowards(currentPos, targetPos, lowestSpeed * Time.deltaTime);
        }

        return newPos;
    }

    void SetCameraTransform(Vector3 position, Vector3 rotation)
    {
        cam.transform.localPosition = position;
        cam.transform.localRotation = Quaternion.Euler(rotation);
    }

    public void ResetCameraOnRig()
    {
        cam.transform.position = this.transform.position;
        cam.transform.rotation = this.transform.rotation;
    }

    void SetCameraState(camStates desiredState)
    {
        cameraState = desiredState;
    }
}

public enum cameraStates
{
    STATE_PLAYERORBIT,
    STATE_DIRFOCUS,
    STATE_POIFOCUS,
    STATE_NULL,
    STATE_COVER,
    STATE_COVERAIM,
    STATE_PLAYERAIM,
    STATE_DETACHED,
    STATE_PLAYINGANIM,
    STATE_LERPING,
    STATE_JUSTORBIT,
    STATE_JUSTPOS,
    STATE_LERPDIRFOCUS,
    STATE_CCTV,
    STATE_PUZZLELERPING,
    STATE_PUZZLECCTV,
    STATE_PUZZLELERPDIRFOCUS,
    STATE_ORBITDISTANCED,
    STATE_GENERATORMINIGAME,
    STATE_NOTHING,
    STATE_ROTATETOLOOKAT,
    d_PlayerRigOrbit,
    d_PlayerRigOrbit_UpdatePosition
};
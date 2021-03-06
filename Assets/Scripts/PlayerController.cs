﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //TODO: Add behavior for expanding collision check on push to include pushed object, fix shooting
    public bool takeInput;
    public float crouchSpeed = 1.5f;
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float gravity = -12f;
    public float jumpHeight = 1f;

    [Range (0,1)]
    public float airControlPercent;

    //Time it takes to get from current value to target value
    public float turnSmoothTime = .1f;
    float turnSmoothVelocity;

    public float speedSmoothTime = .1f;
    float speedSmoothVelocity;
    public static float currentSpeed;
    float velocityY;

    Animator animator;
    Transform cameraT;
    CharacterController controller;

    bool inAutoCrouchArea;
    public static GameObject triggerCollidingWith;
    public GameObject pushableCollidingWith;
    public GameObject currentPush;
    public GameObject coverCollidingWith;
    public GameObject currentCover;
    bool inShortCover;
    public float coverCooldown = 0;

    public float colCenter = .85f;
    public float colHeight = 1.7f;
    public float colBoundsHeight;

    public static MoveState thisMoveState = MoveState.STATE_REGULAR;
    public static Vector3 scriptedMovementTarget;

    public float groundCheckDistance = 0.1f;

    // Used to disable movement for a short duration after getting grounded
    float lastTimeInAir;
    bool isGrounded;

    // Use this for initialization
    void Awake () {
        animator = GetComponent<Animator>();
        cameraT = Camera.main.transform;

        controller = GetComponent<CharacterController>();
        controller.height = colHeight;
        colBoundsHeight = controller.bounds.extents.y;
	}

    // Update is called once per frame
    void Update()
    {
        if (coverCooldown > 0) {
            coverCooldown -= Time.deltaTime;
        }

        if (takeInput)
        {
            switch (thisMoveState) {

                case MoveState.STATE_SCRIPTEDMOVEMENT:

                    // We don't care about the y axis to simplify the logic
                    var target = new Vector2(scriptedMovementTarget.x, scriptedMovementTarget.z);
                    var currentPos = new Vector2(transform.position.x, transform.position.z);

                    var dir = target - currentPos;
                    Move(dir.normalized, false, false, false, true);

                    if (Vector2.Distance(target, currentPos) < 0.1f)
                        thisMoveState = MoveState.STATE_REGULAR;

                    break;

                case MoveState.STATE_REGULAR:
                    Move(CharacterUserControl.inputDir, CharacterUserControl.runInput, CharacterUserControl.crouchInput, CharacterUserControl.jumpInput);
                    UpdateCrouch(CharacterUserControl.crouchInput);

                    //if there is a pushable
                    if (pushableCollidingWith)
                    {
                        // Get the closest point on the bounds of the cover collider  
                        // The position on the players legs
                        var playerPos = transform.position + new Vector3(0, controller.height / 2, 0);
                        var PushPoint = pushableCollidingWith.GetComponent<Collider>().ClosestPoint(playerPos);
                       // pushableText.enabled = true;
                       // pushableText.text = "Press Q to Begin Push";
                       // pushableText.rectTransform.position = new Vector3(PushPoint.x, PushPoint.y + 1f, PushPoint.z);
                       // pushableText.transform.LookAt(pushableText.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    }
                    else if(!pushableCollidingWith)
                       // pushableText.enabled = false;

                    //if there is cover
                    if (coverCollidingWith)
                    {
                        // Get the closest point on the bounds of the cover collider  
                        // The position on the players legs
                        var playerPos = transform.position + new Vector3(0, controller.height / 2, 0);
                        var CoverPoint = coverCollidingWith.GetComponent<Collider>().ClosestPoint(playerPos);
                       // coverText.enabled = true;
                       // coverText.text = "Press Q to Enter Cover";
                       // coverText.rectTransform.position = new Vector3(CoverPoint.x, CoverPoint.y + 1f, CoverPoint.z);
                      //  coverText.transform.LookAt(coverText.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    }
                    else if(!coverCollidingWith)
                      //  coverText.enabled = false;


                    if (Interesting.looking) {
                        thisMoveState = MoveState.STATE_FOCUS;
                    }
                    if (CharacterUserControl.contextSensitiveInput && coverCollidingWith && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentCover = coverCollidingWith;
                        thisMoveState = MoveState.STATE_COVER;
                        print("entered cover");                            
                    }
                    //MAKE SURE THIS STATE TRANSITION WORKS
                    else if (CharacterUserControl.contextSensitiveInput && pushableCollidingWith && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentPush = pushableCollidingWith;
                        thisMoveState = MoveState.STATE_PUSHING;
                        print("entered pushing");
                    }else if (CharacterUserControl.aimInput) {
                        thisMoveState = MoveState.STATE_REGULARAIM;
                    }

                    break;

                case MoveState.STATE_FOCUS:
                    Move(CharacterUserControl.inputDir, false, CharacterUserControl.crouchInput, false);

                    //Transition
                    if (!Interesting.looking)
                    {
                        thisMoveState = MoveState.STATE_REGULAR;
                    }
                    break;

                case MoveState.STATE_DIRFOCUS:
                    Move(CharacterUserControl.inputDir, false, CharacterUserControl.crouchInput, false);
                    break;

                case MoveState.STATE_COVER:
                    CoverMove(CharacterUserControl.inputDir, currentCover);

                    UpdateCrouch(!IsObjectTallerThanPlayer(currentCover));

                   // pushableText.enabled = false;
                   // coverText.enabled = false;

                    if (CharacterUserControl.aimInput) {
                        thisMoveState = MoveState.STATE_COVERAIM;
                    }
                    else if (CharacterUserControl.contextSensitiveInput && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentCover = null;
                        thisMoveState = MoveState.STATE_REGULAR;
                        print("exited cover");                        
                    }
                    break;

                case MoveState.STATE_COVERAIM:
                    UpdateCrouch(!IsObjectTallerThanPlayer(currentCover));

                    CoverMoveAim(CharacterUserControl.inputDir, currentCover);
                    if (!CharacterUserControl.aimInput)
                    {
                        thisMoveState = MoveState.STATE_COVER;
                    }
                    else if (CharacterUserControl.contextSensitiveInput && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentCover = null;
                        thisMoveState = MoveState.STATE_REGULAR;
                        print("exited cover");
                    }
                    break;

                case MoveState.STATE_REGULARAIM:
                    AimMove(CharacterUserControl.inputDir);

                    if (!CharacterUserControl.aimInput) thisMoveState = MoveState.STATE_REGULAR;
                    break;

                case MoveState.STATE_PUSHING:
                    ObjectMove(CharacterUserControl.inputDir, currentPush);
                   // pushableText.enabled = false;
                   // coverText.enabled = false;

                    if (CharacterUserControl.contextSensitiveInput && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentPush = null;
                        thisMoveState = MoveState.STATE_REGULAR;
                        print("exited cover");
                    }
                    break;
            }
            


        } else {
            controller.Move(Vector3.down * 0.1f);
        }
        //Animation 
        //animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        CheckGroundStatus();

        UpdateAnimator();
    }

    void UpdateAnimator() {

        var inSmallCover = ((thisMoveState == MoveState.STATE_COVER || thisMoveState == MoveState.STATE_COVERAIM) && !IsObjectTallerThanPlayer(currentCover));

        bool crouching = false;
        if ((thisMoveState == MoveState.STATE_REGULAR && CharacterUserControl.crouchInput) || inSmallCover) {
            animator.SetBool("Crouch", true);
            crouching = true;
        } else {
            animator.SetBool("Crouch", false);
        }

        //Animation the movement
        // Tweak this if the animation and movement is off
        float forwardAmount = transform.InverseTransformDirection(controller.velocity).z;
        if (!crouching)
            forwardAmount /= 3;

        animator.SetFloat("Forward", forwardAmount);

        animator.SetBool("OnGround", isGrounded);
        if (!isGrounded) {
            animator.SetFloat("Jump", velocityY);
        }

        // Copy pasted from ThirdPersonCharacter.cs (more or less):
        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        const float runCycleOffset = 0.2f;
        float runCycle =
            Mathf.Repeat(
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleOffset, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * forwardAmount;
        if (isGrounded) {
            animator.SetFloat("JumpLeg", jumpLeg);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Trigger") {

        }
        
    }

    void OnTriggerStay(Collider col) {
        if (col.transform.tag == "Cover")
        {
            coverCollidingWith = col.gameObject;
        }
        if (col.transform.tag == "Trigger")
        {
            triggerCollidingWith = col.gameObject;
        }
        if (col.transform.tag == "Pushable") {
            pushableCollidingWith = col.gameObject;
        }
        if(col.transform.tag == "AutoCrouchArea") 
        {
            inAutoCrouchArea = true;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.transform.tag == "Cover") {
            coverCollidingWith = null;
        }
        if (col.transform.tag == "Pushable") {
            pushableCollidingWith = null;
        }
        if (col.transform.tag == "Trigger")
        {
            triggerCollidingWith = null;
        }
        if (col.transform.tag == "AutoCrouchArea") 
        {
            inAutoCrouchArea = false;
        }
    }

    float GetAngleBetween3PointsHor(Vector3 a, Vector3 b)
    {
        float theta = Mathf.Atan2(b.x - a.x, b.z - a.z);
        float angle = theta * 180 / Mathf.PI;
        return angle;
    }

    void UpdateCrouch(bool crouching) {
        var currentHeight = controller.height;

        float crouchHeight = 1;
        if (crouching) {
            if(currentHeight != crouchHeight) {
                // Change the height and center of the player controller
                // So that it stays on the ground when we decrease the height
                var delta = controller.height - crouchHeight;
                var center = controller.center;
                center.y -= delta/2;
                controller.center = center;

                controller.height = crouchHeight;
            }
        } else {
            if(controller.height != colHeight) {
                // Change the height and center of the player controller
                var delta = controller.height - colHeight;
                var center = controller.center;
                center.y -= delta / 2;
                controller.center = center;

                controller.height = crouchHeight;
            }
            GetComponent<CharacterController>().height = colHeight;
        }
    }

    bool IsObjectTallerThanPlayer(GameObject obj) {
        var coll = obj.GetComponent<Collider>();
        var bounds = coll.bounds;
        if (bounds.extents.y > colBoundsHeight) return true;

        return false;
    }

    void Move(Vector2 inputDir, bool running, bool crouching, bool jumped, bool ignoreCameraRotation = false)
    {
        //Updating the rotation of the character
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
            if (!ignoreCameraRotation)
                targetRotation += cameraT.eulerAngles.y;

            if (float.IsNaN(turnSmoothVelocity)) {
                turnSmoothVelocity = 0;
                Debug.LogWarning("TURN SMOOTH VELOCITY DECIDED TO BE NAN BUT I SAVED THE DAY!");
            }
                

            //print("Regular Movement Target Rot: " + targetRotation);
            var smoothDamp = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
            var newRot = Vector3.up * smoothDamp;

            //Debug.Log(newRot + " : " + smoothDamp + " : " + GetModifiedSmoothTime(turnSmoothTime) + " : " + transform.eulerAngles.y + " : " + targetRotation + " : " + inputDir + " : " + turnSmoothVelocity);
            transform.eulerAngles = newRot;
        }

        //If running, the target speed = run speed, else the target speed = walk speed. All in the direction of the character
        float targetSpeed = ((crouching) ? crouchSpeed : (running) ? runSpeed : walkSpeed) * inputDir.magnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
        //transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        if (isGrounded && Time.time - lastTimeInAir < 0.3f) {
            velocity.x = 0;
            velocity.z = 0;
        }

        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0;
        }

        if (jumped) Jump();
    }

    void AimMove(Vector2 inputDir) {

        float targetRotation = cameraT.eulerAngles.y;

        //print("Regular Movement Target Rot: " + targetRotation);
        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime) / 2);

        //If running, the target speed = run speed, else the target speed = walk speed. All in the direction of the character
        float targetSpeed  = crouchSpeed * inputDir.magnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity;


        Vector3 velocity = transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y)) * currentSpeed + Vector3.up * velocityY;
        if(isGrounded && Time.time - lastTimeInAir < 0.3f) {
            velocity.x = 0;
            velocity.z = 0;
        }

        //transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);


        controller.Move(velocity * Time.deltaTime);

        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded) {
            velocityY = 0;
        }
    }

    void CoverMoveAim(Vector2 inputDir, GameObject cover)
    {

        // The position on the players legs, don't actually have to be at the legs level, but did it so that it's easier to see
        var playerPos = transform.position + new Vector3(0, controller.height / 2, 0);

        // Get the closest point on the bounds of the cover collider        
        var coverPoint = cover.GetComponent<Collider>().ClosestPoint(playerPos);

        // Use these 2 positions, (plus 1 on the y axis for the third variable in the normal function) to get a normal, this is the vector we can move along
        var normal = GetNormal(playerPos, coverPoint, playerPos + new Vector3(0, 1, 0));

        // Origins of the rays, one on each side of the player in relation to the cover
        var rayOrigin1 = playerPos + (normal * 0.2f);
        var rayOrigin2 = playerPos - (normal * 0.2f);

        Vector3 dir = coverPoint - new Vector3(playerPos.x, coverPoint.y, playerPos.z);
        dir.Normalize();

        // We use this plane to see which direction were running along the cover
        var plane = new Plane(normal, Vector3.zero);

        // Cast 2 rays on each side of the player, if one of these are off we restrict movement on that side of the plane
        bool canMoveRight = false;
        if (Physics.Raycast(rayOrigin1, dir, 1))
        {
            Debug.DrawLine(rayOrigin1, rayOrigin1 + (dir * 1), Color.green);
            canMoveRight = true;
        }
        else
        {
            Debug.DrawLine(rayOrigin1, rayOrigin1 + (dir * 1), Color.red);
        }

        bool canMoveLeft = false;
        if (Physics.Raycast(rayOrigin2, dir, 1))
        {
            Debug.DrawLine(rayOrigin2, rayOrigin2 + (dir * 1), Color.green);
            canMoveLeft = true;
        }
        else
        {
            Debug.DrawLine(rayOrigin2, rayOrigin2 + (dir * 1), Color.red);
        }

        float targetRotation = cameraT.eulerAngles.y;

        //print("Regular Movement Target Rot: " + targetRotation);
        //transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        transform.eulerAngles = Vector3.up * targetRotation;

        //If running, the target speed = run speed, else the target speed = walk speed. All in the direction of the character
        float targetSpeed = crouchSpeed * inputDir.magnitude;

        currentSpeed = targetSpeed;

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y)) * currentSpeed;
        velocity = Vector3.Project(velocity, normal);
        velocity.y += velocityY;

        // Restrict the movement if one of the rays missed
        // We do this by checking which direction were running along the normal
        if (!canMoveRight && plane.GetSide(velocity))
            velocity = Vector3.zero;

        if (!canMoveLeft && !plane.GetSide(velocity))
            velocity = Vector3.zero;


        //transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }

    
    void CoverMove(Vector2 inputDir, GameObject cover)
    {
        var playerPos = transform.position + new Vector3(0, controller.height / 2, 0);
        var coverPoint = cover.GetComponent<Collider>().ClosestPoint(playerPos);

        var normal = GetNormal(playerPos, coverPoint, playerPos + new Vector3(0, 1, 0));

        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            //print("Regular Movement Target Rot: " + targetRotation);
            transform.eulerAngles = Vector3.up * targetRotation;
        }

        //If running, the target speed = run speed, else the target speed = walk speed. All in the direction of the character
        float targetSpeed = crouchSpeed * inputDir.magnitude;

        currentSpeed = targetSpeed;

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * currentSpeed;
        velocity = Vector3.Project(velocity, normal);
        velocity.y += velocityY;
        //transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }
    

    void ObjectMove(Vector2 inputDir, GameObject cover)
    {
        var playerPos = transform.position + new Vector3(0, controller.height / 2, 0);
        var coverPoint = cover.GetComponent<Collider>().ClosestPoint(playerPos);

        // New normal calculation, simpler
        coverPoint.y = playerPos.y;
        var normal = (coverPoint - playerPos).normalized;

        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            //print("Regular Movement Target Rot: " + targetRotation);
            transform.eulerAngles = Vector3.up * targetRotation;
        }

        //If running, the target speed = run speed, else the target speed = walk speed. All in the direction of the character
        float targetSpeed = crouchSpeed * inputDir.magnitude;

        currentSpeed = targetSpeed;

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * currentSpeed;
        // New movement code, also move the cover object
        velocity = Vector3.Project(velocity, normal) * Time.deltaTime;
        velocity.y += velocityY;

        controller.Move(velocity);
        cover.transform.position += new Vector3(velocity.x, 0, velocity.z);

        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }
    
    void Jump() {
        if (controller.isGrounded) {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            velocityY = jumpVelocity;
        }
    }

    float GetModifiedSmoothTime(float smoothTime) {
        if (controller.isGrounded) {
            return smoothTime;
        }

        if (airControlPercent == 0) {
            return float.MaxValue;
        }
        return smoothTime / airControlPercent;
    }   

    void CheckGroundStatus() {
        if (controller.isGrounded) {
            isGrounded = true;
            return;
        }

#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif

        var layers = Physics.AllLayers;
        layers = layers ^ 1 << gameObject.layer; // Ignore the layer this object is on

        Vector3 halfExtents = new Vector3(controller.bounds.extents.x, groundCheckDistance, controller.bounds.extents.z);

        if (Physics.OverlapBox(transform.position, halfExtents, Quaternion.identity, layers, QueryTriggerInteraction.Ignore).Length > 0) {
            isGrounded = true;
        } else {
            isGrounded = false;
            lastTimeInAir = Time.time;
        }
    }
}

public enum MoveState
{
    STATE_REGULAR,
    STATE_REGULARAIM,
    STATE_FOCUS,
    STATE_CROUCH,
    STATE_COVER,
    STATE_COVERAIM,
    STATE_CLIMBING,
    STATE_PUSHING,
    STATE_NULL,
    STATE_DIRFOCUS,
    STATE_SCRIPTEDMOVEMENT
};
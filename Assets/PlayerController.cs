﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

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
    float currentSpeed;
    float velocityY;

    Animator animator;
    Transform cameraT;
    CharacterController controller;

    public bool inCover;
    public float testForwardRot;
    public GameObject currentCover;

    enum MoveState {
        STATE_REGULAR,
        STATE_COVER
    };
    MoveState thisMoveState;

	// Use this for initialization
	void Awake () {
        animator = GetComponent<Animator>();
        cameraT = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        LevelScript.DCharInput += DisableInput;
        LevelScript.ECharInput += EnableInput;
	}

    void Start() {
        thisMoveState = MoveState.STATE_REGULAR;
    }

    // Update is called once per frame
    void Update()
    {
        bool running = false;
        bool crouching = false;
        //bool looking = false;
        Vector2 input = new Vector2(0,0);
        Vector2 inputDir = new Vector2(0,0);
        float animationSpeedPercent = 0;

        if (takeInput)
        {
            //Direction to face
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            inputDir = input.normalized;

            running = Input.GetKey(KeyCode.LeftShift);
            crouching = Input.GetKey(KeyCode.LeftControl);
            
            //When the player checks for cover, they will need to get the piece of cover they're colliding with, and then get that piece of cover's forward direction in relation to the Player
            if (Input.GetKeyDown(KeyCode.K)) {
                //inCover = !inCover;
            }
            
            if (inCover)
            {
                thisMoveState = MoveState.STATE_COVER;
            }
            else if (!inCover) {
                thisMoveState = MoveState.STATE_REGULAR;
            }

            switch (thisMoveState) {
                case MoveState.STATE_REGULAR:
                    //GetAngleBetween3PointsHor(transform.position, );
                    Move(inputDir, running, crouching);

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Jump();
                    }
                    break;
                case MoveState.STATE_COVER:
                    CoverMove(inputDir, currentCover);
                    break;

            }
            
            //Animation comes last
            animationSpeedPercent = ((running) ? currentSpeed / runSpeed : (crouching) ? currentSpeed / crouchSpeed : currentSpeed / walkSpeed * .5f);
        }

        Debug.DrawLine(transform.localPosition, transform.forward + transform.localPosition);
        if (currentCover != null)
        {
            Debug.DrawLine(transform.localPosition, currentCover.transform.localPosition - transform.localPosition);
            Debug.DrawLine(transform.localPosition, currentCover.transform.localPosition + transform.localPosition);
            Debug.DrawLine(transform.localPosition, currentCover.transform.localPosition + transform.position);
            Debug.DrawLine(transform.localPosition, currentCover.transform.localPosition - transform.position);
            Debug.DrawLine(transform.localPosition, currentCover.transform.localPosition);
            Debug.DrawLine(transform.localPosition, currentCover.transform.position);
            Debug.DrawLine(transform.localPosition, currentCover.transform.position + transform.localPosition);
            Debug.DrawLine(transform.localPosition, currentCover.transform.position + transform.position);
            Debug.DrawLine(transform.localPosition, currentCover.transform.position - transform.localPosition);
            Debug.DrawLine(transform.localPosition, currentCover.transform.position - transform.position);
        }
        //Animation 
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
    }

    void OnTriggerStay(Collider col) {
        if (col.transform.tag == "Cover")
        {
            if (Input.GetKey(KeyCode.K))
            {
                //Get the angle between the player and the cover
                //float tester = Vector3.Angle(transform.forward, col.transform.position - transform.position);
                //print("Angle between: " + tester);
                if (!inCover)
                {
                    currentCover = col.transform.gameObject;
                    print("entered cover");
                    inCover = true;
                }
                else
                {
                    currentCover = null;
                    print("exited cover");
                    inCover = false;
                }
            }
        }
    }

    float GetAngleBetween3PointsHor(Vector3 a, Vector3 b)
    {
        float theta = Mathf.Atan2(b.x - a.x, b.z - a.z);
        float angle = theta * 180 / Mathf.PI;
        return angle;
    }

    void Move(Vector2 inputDir, bool running, bool crouching) {
        //Updating the rotation of the character
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            //print("Regular Movement Target Rot: " + targetRotation);
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }

        //If running, the target speed = run speed, else the target speed = walk speed. All in the direction of the character
        float targetSpeed = ((running) ? runSpeed : (crouching) ? crouchSpeed : walkSpeed) * inputDir.magnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
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
        var coverPoint = cover.GetComponent<Collider>().ClosestPointOnBounds(playerPos);
        Debug.Log(playerPos + ":" + coverPoint);
        Debug.DrawLine(playerPos, coverPoint, Color.red);

        var normal = GetNormal(playerPos, coverPoint, playerPos + new Vector3(0, 1, 0));
        Debug.Log(normal);
        Debug.DrawLine(playerPos, playerPos + (normal * 10), Color.blue);

        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            print("Regular Movement Target Rot: " + targetRotation);
            transform.eulerAngles = Vector3.up * targetRotation;
        }

        //If running, the target speed = run speed, else the target speed = walk speed. All in the direction of the character
        float targetSpeed = crouchSpeed * inputDir.magnitude;

        currentSpeed = targetSpeed;

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
        velocity = Vector3.Project(velocity, normal);
        //transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        controller.Move(velocity * Time.deltaTime);
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

    void EnableInput() {
        takeInput = true;
    }

    void DisableInput() {
        takeInput = false;
    }
}

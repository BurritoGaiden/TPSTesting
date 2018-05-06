using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    bool runInput;
    public static bool crouchInput;
    bool jumpInput;
    bool coverInput;

    public static GameObject triggerCollidingWith;
    public GameObject pushableCollidingWith;
    GameObject currentPush;
    public GameObject coverCollidingWith;
    GameObject currentCover;
    float coverCooldown = 0;

    public Text pushableText;
    public Text coverText;

    public float colCenter = .85f;
    public float colHeight = 1.7f;

    public static MoveState thisMoveState = MoveState.STATE_REGULAR;

	// Use this for initialization
	void Awake () {
        animator = GetComponent<Animator>();
        cameraT = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        LevelScript.DCharInput += DisableInput;
        LevelScript.ECharInput += EnableInput;
	}

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(0,0);
        Vector2 inputDir = new Vector2(0,0);
        float animationSpeedPercent = 0;

        if (coverCooldown > 0) {
            coverCooldown -= Time.deltaTime;
        }

        if (takeInput)
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            inputDir = input.normalized;

            runInput = Input.GetKey(KeyCode.LeftShift);
            crouchInput = Input.GetKey(KeyCode.LeftControl);
            //jumpInput = Input.GetKeyDown(KeyCode.Space);
            coverInput = Input.GetKeyDown(KeyCode.Q);

            switch (thisMoveState) {

                case MoveState.STATE_REGULAR:
                    Move(inputDir, runInput, crouchInput, jumpInput);

                    if (crouchInput)
                    {
                        //GetComponent<CharacterController>().center = new Vector3(0,0,.1f);
                        GetComponent<CharacterController>().height = 1;
                    }
                    else {
                        //GetComponent<CharacterController>().center = new Vector3(0, colCenter, .1f);
                        GetComponent<CharacterController>().height = colHeight;
                    }

                    //if there is a pushable
                    if (pushableCollidingWith)
                    {
                        // Get the closest point on the bounds of the cover collider  
                        // The position on the players legs
                        var playerPos = transform.position + new Vector3(0, controller.height / 2, 0);
                        var PushPoint = pushableCollidingWith.GetComponent<Collider>().ClosestPoint(playerPos);
                        pushableText.enabled = true;
                        pushableText.text = "Press Q to Begin Push";
                        pushableText.rectTransform.position = new Vector3(PushPoint.x, PushPoint.y + 1f, PushPoint.z);
                        pushableText.transform.LookAt(pushableText.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    }
                    else if(!pushableCollidingWith)
                        pushableText.enabled = false;

                    //if there is a pushable
                    if (coverCollidingWith)
                    {
                        // Get the closest point on the bounds of the cover collider  
                        // The position on the players legs
                        var playerPos = transform.position + new Vector3(0, controller.height / 2, 0);
                        var CoverPoint = coverCollidingWith.GetComponent<Collider>().ClosestPoint(playerPos);
                        coverText.enabled = true;
                        coverText.text = "Press Q to Enter Cover";
                        coverText.rectTransform.position = new Vector3(CoverPoint.x, CoverPoint.y + 1f, CoverPoint.z);
                        coverText.transform.LookAt(coverText.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    }
                    else if(!coverCollidingWith)
                        coverText.enabled = false;


                    if (Interesting.looking) {
                        thisMoveState = MoveState.STATE_FOCUS;
                    }
                    if (coverInput && coverCollidingWith && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentCover = coverCollidingWith;
                        thisMoveState = MoveState.STATE_COVER;
                        print("entered cover");                            
                    }
                    //MAKE SURE THIS STATE TRANSITION WORKS
                    else if (coverInput && pushableCollidingWith && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentPush = pushableCollidingWith;
                        thisMoveState = MoveState.STATE_PUSHING;
                        print("entered pushing");
                    }
                    break;

                case MoveState.STATE_FOCUS:
                    Move(inputDir, false, crouchInput, false);

                    //Transition
                    if (!Interesting.looking)
                    {
                        thisMoveState = MoveState.STATE_REGULAR;
                    }
                    break;

                case MoveState.STATE_COVER:
                    CoverMove(inputDir, currentCover);
                    pushableText.enabled = false;
                    coverText.enabled = false;

                    if (Killing.aiming) {
                        thisMoveState = MoveState.STATE_COVERAIM;
                    }
                    else if (coverInput && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentCover = null;
                        thisMoveState = MoveState.STATE_REGULAR;
                        print("exited cover");                        
                    }
                    break;

                case MoveState.STATE_COVERAIM:
                    CoverMoveAim(inputDir, currentCover);
                    if (!Killing.aiming)
                    {
                        thisMoveState = MoveState.STATE_COVER;
                    }
                    else if (coverInput && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentCover = null;
                        thisMoveState = MoveState.STATE_REGULAR;
                        print("exited cover");
                    }
                    break;

                //MAKE SURE PUSHING STATE WORKS
                //MAKE SURE PUSHING STATE CAN BE EXITED
                case MoveState.STATE_PUSHING:
                    ObjectMove(inputDir, currentPush);
                    pushableText.enabled = false;
                    coverText.enabled = false;

                    if (coverInput && coverCooldown <= 0)
                    {
                        coverCooldown = 1.2f;
                        currentPush = null;
                        thisMoveState = MoveState.STATE_REGULAR;
                        print("exited cover");
                    }
                    break;
            }
            
            //Animation the movement
            animationSpeedPercent = ((runInput) ? currentSpeed / runSpeed : (crouchInput) ? currentSpeed / crouchSpeed : currentSpeed / walkSpeed * .5f);
        }        
        //Animation 
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
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
    }

    float GetAngleBetween3PointsHor(Vector3 a, Vector3 b)
    {
        float theta = Mathf.Atan2(b.x - a.x, b.z - a.z);
        float angle = theta * 180 / Mathf.PI;
        return angle;
    }

    void Move(Vector2 inputDir, bool running, bool crouching, bool jumped) {
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

        if (jumped) Jump();
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

        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
        velocity = Vector3.Project(velocity, normal);

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

        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
        // New movement code, also move the cover object
        velocity = Vector3.Project(velocity, normal) * Time.deltaTime;
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

    void EnableInput() {
        takeInput = true;
    }

    void DisableInput() {
        takeInput = false;
    }
}

public enum MoveState
{
    STATE_REGULAR,
    STATE_FOCUS,
    STATE_CROUCH,
    STATE_COVER,
    STATE_COVERAIM,
    STATE_CLIMBING,
    STATE_PUSHING,
    STATE_NULL
};
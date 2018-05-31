using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUserControl : MonoBehaviour {

    public static Vector2 input;
    public static Vector2 inputDir;
    public static bool jumpInput;
    public static bool runInput;
    public static bool crouchInput;
    public static bool aimInput;
    public static bool fireInput;
    public static bool contextSensitiveInput;

    public static bool takeInput;

    void Awake() {
        LevelScript.b_SetCharacterInput += SetInput;
    }
    // Update is called once per frame
    void Update()
    {
        if (takeInput)
        {
            input = new Vector2(0, 0);
            inputDir = new Vector2(0, 0);

            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            inputDir = input.normalized;

            runInput = Input.GetKey(KeyCode.LeftShift);
            fireInput = Input.GetKey(KeyCode.Mouse0);
            aimInput = Input.GetKey(KeyCode.Mouse1);
            crouchInput = Input.GetKey(KeyCode.LeftControl);
            contextSensitiveInput = Input.GetKeyDown(KeyCode.Q);
        }
    }

    void SetInput(bool desiredBool) { takeInput = desiredBool; }
}

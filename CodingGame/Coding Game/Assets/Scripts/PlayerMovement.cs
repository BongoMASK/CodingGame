using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {
    int damage, currentHealth;
    public CharacterController2D controller;

    float horizontalMove = 0f;
    public float runSpeed = 40f;

    bool jump = false;
    bool crouch = false;

    private void Start() {

    }


    void Update() {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        //makes player jump
        if (Input.GetKeyDown(KeyCode.Space)) {
            jump = true;
        }

        //makes player crouch (inactive)
        if (Input.GetKeyDown(KeyCode.S)) {
            crouch = true;
        }
        else if (Input.GetKeyUp(KeyCode.S)) {
            crouch = false;
        }
    }

    private void FixedUpdate() {
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
        jump = false;
    }
}

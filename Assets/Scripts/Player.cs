using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float jumpHeight = 10000f;
    private float jumpDelay = 0.3f;

    private float maxSpeed = 6f;
    private float moveSpeed = 800000f;
    private float cmMultiplier = 3f;

    private float mouseSensitivity = 150f;
    private float xRotation = 0f;

    private bool isGrounded;
    private float groundCheckRadius = 0.1f;
    private bool readyToJump = true;

    public Camera playerCam;
    public Rigidbody rb;
    public Transform groundCheck;
    public LayerMask whatIsGround;

    private float x;
    private float y;
    private bool jumping;

    void Start() {
        // Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        isGrounded = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, whatIsGround).Length > 0;
        keys();
        look();
        movement();
        jump();
    }

    private void keys(){
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
    }

    private void movement(){
        counterMovement();

        rb.AddForce(transform.forward * y * moveSpeed * Time.deltaTime);
        rb.AddForce(transform.right * x * moveSpeed * Time.deltaTime);
    }

    private void jump(){
        if(jumping && isGrounded && readyToJump){
            readyToJump = false;
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            Invoke("restartJump", jumpDelay);
        }
    }

    private void look(){
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void restartJump(){
        readyToJump = true;
    }

    private void counterMovement(){
        if(isGrounded){
            Vector2 mag = getMagnitudeXAndY();

            if(x == 0) rb.AddForce(transform.right * moveSpeed * Time.deltaTime  * -mag.x * cmMultiplier);
            if(y == 0) rb.AddForce(transform.forward * moveSpeed * Time.deltaTime  * -mag.y * cmMultiplier);
        }

        if(rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    private Vector2 getMagnitudeXAndY(){
        float lookAngle = transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;
        float magnitude = rb.velocity.magnitude;
        float magY = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float magX = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(magX, magY);
    }
}

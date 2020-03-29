using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float jumpHeight = 20000f;
    private float jumpDelay = 0.3f;

    private float maxSpeed = 6f;
    private float moveSpeed = 800000f;
    private float cmMultiplier = 3f;

    private float mouseSensitivity = 150f;
    private float xRotation = 0f;

    private bool isGrounded;
    private float groundCheckRadius = 0.1f;
    private bool readyToJump = true;

    public Rigidbody rb;
    public Transform groundCheck;
    public LayerMask whatIsGround;

    public Quaternion camRotation;

    public Keys keys { get; set; }

    void Start() {
        keys = new Keys();
    }

    void Update() {
        isGrounded = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, whatIsGround).Length > 0;
        look();
        movement();
        jump();
    }

    // void FixedUpdate() {
    //     sendKeys();
    // }

    // private void sendKeys(){
    //     keys.updateKeys();
    // }

    public void setKeys(Keys keys){
        this.keys = keys;
    }

    private void look(){
        float mX = keys.mouseX * mouseSensitivity * Time.deltaTime;
        float mY = keys.mouseY * mouseSensitivity * Time.deltaTime;

        xRotation -= mY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mX);
    }

    private void movement() {
        if(keys.shift) return;
        counterMovement();
        rb.AddForce(transform.forward * keys.y * moveSpeed * Time.deltaTime);
        rb.AddForce(transform.right * keys.x * moveSpeed * Time.deltaTime);
    }

    private void jump() {
        if(keys.jumping && isGrounded && readyToJump){
            readyToJump = false;
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            Invoke("restartJump", jumpDelay);
        }
    }

    private void restartJump() {
        readyToJump = true;
    }

    private void counterMovement() {
        if (isGrounded) {
            Vector2 mag = getMagnitudeXAndY();

            if (keys.x == 0) rb.AddForce(transform.right * moveSpeed * Time.deltaTime  * -mag.x * cmMultiplier);
            if (keys.y == 0) rb.AddForce(transform.forward * moveSpeed * Time.deltaTime  * -mag.y * cmMultiplier);
        }

        if(rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    private Vector2 getMagnitudeXAndY() {
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

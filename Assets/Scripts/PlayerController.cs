using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    public Rigidbody rb;
    public Transform groundCheck;
    public LayerMask whatIsGround;

    private float x;
    private float y;
    private bool jumping;

    void Update() {
        isGrounded = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, whatIsGround).Length > 0;
        movement();
        jump();
    }

    public void setKeys(float x, float y, bool jumping, Quaternion rotation){
        this.x = x;
        this.y = y;
        this.jumping = jumping;
        transform.rotation = rotation;
    }

    private void movement() {
        counterMovement();
        rb.AddForce(transform.forward * y * moveSpeed * Time.deltaTime);
        rb.AddForce(transform.right * x * moveSpeed * Time.deltaTime);
    }

    private void jump() {
        if(jumping && isGrounded && readyToJump){
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

            if (x == 0) rb.AddForce(transform.right * moveSpeed * Time.deltaTime  * -mag.x * cmMultiplier);
            if (y == 0) rb.AddForce(transform.forward * moveSpeed * Time.deltaTime  * -mag.y * cmMultiplier);
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

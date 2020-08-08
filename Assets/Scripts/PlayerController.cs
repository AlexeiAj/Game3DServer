using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public GameObject player;
    public Camera playerCam;
    public Keys keys { get; set; }
    public string username = "";
    public int id = -1;

    private float dashExecutionTimeDelay = 0.1f;
    private float dashForce = 2000f;
    private float dashDelay = 0.8f;
    private bool finishDashDelay = true;
    private bool isDashing = false;

    private float jumpHeight = 20000f;
    private float jumpDelay = 0.6f;
    private bool readyToJump = true;
    
    private float maxSpeed = 12f;
    private float moveSpeed = 10f;
    private float mouseSensitivity = 70f;
    private float xRotation = 0f;
    private bool isRunning = false;
    
    private bool isGrounded;
    private float groundCheckRadius = 0.1f;

    public Rigidbody rb;
    public Transform groundCheck;
    public LayerMask whatIsGround;

    public Quaternion camRotation;

    private float damage = 10f;
    private float range = 100f;
    private float fireRate = 2f;
    private float nextTimeToFire = 0f;

    private float health = 100f;
    private float block = 80f;

    void Start() {
        keys = new Keys();
    }

    void FixedUpdate() {
        isGrounded = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, whatIsGround).Length > 0;
        look();
        movement();
        jump();
        dash();
        shoot();
        sendPlayerPosition();
    }

    private void shoot() {
        if(keys.mouseLeft && Time.time >= nextTimeToFire) {
            nextTimeToFire = Time.time + 2f / fireRate;

            RaycastHit hit;
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range)) {
                // Debug.Log(hit.transform.name);
                PlayerController playerController = hit.transform.gameObject.GetComponent<PlayerController>();
                if(playerController) playerController.doDamage(10);
                sendShootImpactLocation(hit.point, Quaternion.LookRotation(hit.normal), playerController != null);
            }
        }
    }

    public void sendShootImpactLocation(Vector3 hitPoint, Quaternion rotationPoint, bool isPlayer) {
        Packet packet = new Packet();
        packet.Write("shootImpactLocationFS");
        packet.Write(id);
        packet.Write(hitPoint);
        packet.Write(rotationPoint);
        packet.Write(isPlayer);

        Server.instance.sendUdpDataToAll(packet);
    }

    private void look(){
        float mX = keys.mouseX * mouseSensitivity * Time.deltaTime;
        float mY = keys.mouseY * mouseSensitivity * Time.deltaTime;

        xRotation -= mY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camRotation = Quaternion.Euler(xRotation, 0, 0);
        playerCam.transform.localRotation = camRotation;
        transform.Rotate(Vector3.up * mX);
    }

    private void movement() {
        running();

        if (isGrounded) {
            Vector3 targetVelocity = new Vector3(keys.x, 0, keys.y);
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= moveSpeed;

            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxSpeed, maxSpeed);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxSpeed, maxSpeed);
            velocityChange.y = 0;
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            
        }

        if (!isGrounded) { 
            if (keys.y != 0) rb.AddForce(transform.forward * keys.y * moveSpeed * 100000f * Time.deltaTime);
            if (keys.x != 0) rb.AddForce(transform.right * keys.x * moveSpeed * 100000f * Time.deltaTime);
            if (rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private void running() {
        bool aux = isRunning;

        isRunning = keys.x != 0 || keys.y != 0;
        if(!isGrounded) isRunning = false;
        if(aux == isRunning) return;
    }

    private void jump() {
        if(keys.jumping && isGrounded && readyToJump){
            readyToJump = false;
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            Invoke("restartJump", jumpDelay);
        }
    }

    private void dash() {
        if(keys.mouseRight && !isGrounded && !isDashing && finishDashDelay){
            isDashing = true;
            finishDashDelay = false;
            Invoke("restartDash", dashExecutionTimeDelay);
            Invoke("resetFinishDashDelay", dashDelay);
        }

        if(isDashing){
            if (keys.y != 0) rb.AddForce(player.transform.forward * keys.y * dashForce, ForceMode.Acceleration);
            if (keys.x != 0) rb.AddForce(player.transform.right * keys.x * dashForce, ForceMode.Acceleration);
        }
    }  

    private void restartJump() {
        readyToJump = true;
    }

    private void restartDash() {
        isDashing = false;
    }

    private void resetFinishDashDelay() {
        finishDashDelay = true;
    }

    public void doDamage(float value) {
        block = block - value <= 0 ? 0 : block - value;
        health = health - (value - (block / value)) <= 0 ? 0 : health - (value - (block / value));

        if(health == 0){
            Packet packet = new Packet();
            packet.Write("killPlayerFS");
            packet.Write(id);
            Server.instance.sendTcpDataToAll(packet);
            player.SetActive(false);
            Invoke("respawnPlayer", 10f);
        }
    }

    public void respawnPlayer() {
        Packet packet = new Packet();
        packet.Write("respawnPlayerFS");
        packet.Write(id);
        player.SetActive(true);
        resetPlayerLife();
        Server.instance.sendTcpDataToAll(packet);
    }

    public void resetPlayerLife() {
        health = 100f;
        block = 80f;
    }

    public void sendPlayerPosition() {
        Packet packet = new Packet();
        packet.Write("playerPositionFS");
        packet.Write(id);
        packet.Write(player.transform.position);
        packet.Write(player.transform.rotation);
        packet.Write(camRotation);
        packet.Write(isGrounded);
        packet.Write(health);
        packet.Write(block);
        packet.Write(keys);

        Server.instance.sendUdpDataToAll(packet);
    }

    // private void updateKeys(){
    //     keys.updateKeys();
    // }

    public void playerKeys(Packet packet) {
        Keys keys = packet.ReadKeys();
        setKeys(keys);
    }

    public void setKeys(Keys keys){
        this.keys = keys;
    }

    public void removePlayer() {
        ThreadManager.ExecuteOnMainThread(() => Destroy(player));
    }

    public void setPlayerPosition(Vector3 position) {
        player.transform.position = position;
    }

    public void setPlayerRotation(Quaternion rotation) {
        player.transform.rotation = rotation;
    }

    public Vector3 getPlayerPosition() {
        return player.transform.position;
    }

    public Quaternion getPlayerRotation() {
        return player.transform.rotation;
    }

    public void setId(int id) {
        this.id = id;
    }

    public void setUsername(string username) {
        this.username = username;
    }
}
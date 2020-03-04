using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client {
    private int id;
    private Tcp tcp;
    private IPEndPoint endPointUdp;
    
    private GameObject player;

    private string username;
    private Vector3 position;
    private Quaternion rotation;

    public Client(int id, Vector3 position, Quaternion rotation) {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
    }

    public void sendTcpData(Packet packet) {
        tcp.sendData(packet);
    }

    public Tcp getTcp() {
        return tcp;
    }

    public void setTcp(Tcp tcp) {
        this.tcp = tcp;
    }

    public int getId() {
        return id;
    }

    public IPEndPoint getEndPointUdp() {
        return endPointUdp;
    }

    public void setEndPointUdp(IPEndPoint endPointUdp) {
        this.endPointUdp = endPointUdp;
    } 

    public Vector3 getPosition() {
        return position;
    }

    public Quaternion getRotation() {
        return rotation;
    }

    public void setPosition(Vector3 position) {
        this.position = position;
    }

    public void setRotation(Quaternion rotation) {
        this.rotation = rotation;
    }

    public string getUsername() {
        return username;
    }

    public void setUsername(string username) {
        this.username = username;
    }

    public void setPlayer(GameObject player) {
        this.player = player;
    }

    public GameObject getPlayer() {
        return player;
    }
}

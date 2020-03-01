using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client {
    private int id;
    private Tcp tcp;
    private IPEndPoint endPointUdp;
    private bool firstConnectionUdp = true;

    public Client(int id) {
        this.id = id;
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

    public bool getFirstConnectionUdp() {
        return firstConnectionUdp;
    }

    public void setFirstConnectionUdp(bool firstConnectionUdp) {
        this.firstConnectionUdp = firstConnectionUdp;
    }
}

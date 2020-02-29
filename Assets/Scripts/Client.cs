using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client {
    private int id;
    private Tcp tcp;

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
}

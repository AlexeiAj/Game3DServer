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
        tcp = new Tcp();
    }

    public Tcp getTcp() {
        return tcp;
    }

    public int getId() {
        return id;
    }
}

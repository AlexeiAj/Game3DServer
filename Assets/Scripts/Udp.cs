using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Udp {
    private IPEndPoint endPoint;
    UdpClient udpListener;
    private bool firstConnection = true;

    public Udp(UdpClient udpListener) {
        this.udpListener = udpListener;
    }

    public void connect() {
        // this.endPoint = new IPEndPoint(IPAddress.Any, port);
        udpListener.BeginReceive(udpReceiveCallback, null);
    }

    private void udpReceiveCallback(System.IAsyncResult result) {
        try {
            IPEndPoint clietEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clietEndPoint);
            udpListener.BeginReceive(udpReceiveCallback, null);

            if(endPoint == null) endPoint = clietEndPoint;

            if (data.Length < 4) {
                Debug.Log("Disconnecting client udp...");
                return;
            }

            handleData(data);
        } catch (System.Exception e) {
            Debug.Log(e);
            Debug.Log("Disconnecting client udp...");
        }
    }

    public void handleData(byte[] data) {
        Packet packet = new Packet(data);

        int i = packet.ReadInt(); //só para remover o id do pacote

        string msg = packet.ReadString();
        string username = packet.ReadString();
        int id = packet.ReadInt();

        Debug.Log("Client udp message: " + msg + " id: " + id + " username: " + username);
        
        if (firstConnection) {
            Server.instance.sendReceiveConnectionByUdp(id);
            firstConnection = false;
        }
    }

    public void sendData(Packet packet) {
        try {
            if (endPoint == null) return;
            udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
        } catch {
            Debug.Log("Err. sending udp to client!");
        }
    }
}

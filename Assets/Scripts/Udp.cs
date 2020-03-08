using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Udp {
    UdpClient udpListener;

    public Udp(UdpClient udpListener) {
        this.udpListener = udpListener;
    }

    public void connect() {
        udpListener.BeginReceive(udpReceiveCallback, null);
    }

    private void udpReceiveCallback(System.IAsyncResult result) {
        try {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
            udpListener.BeginReceive(udpReceiveCallback, null);

            if (data.Length < 4) {
                Debug.Log("Disconnecting client udp...");
                return;
            }

            handleData(data, clientEndPoint);
        } catch (System.Exception e) {
            Debug.Log(e);
            Debug.Log("Disconnecting client udp...");
        }
    }

    public void handleData(byte[] data, IPEndPoint clientEndPoint) {
        Packet packet = new Packet(data);

        int i = packet.ReadInt(); //só para remover o id do pacote

        string method = packet.ReadString();
        int id = packet.ReadInt();

        Client client = Server.instance.getClientById(id);
        if (client.getEndPointUdp() == null) {
            client.setEndPointUdp(clientEndPoint);
        }

        if (method.Equals("newConnectionUDP")) {
            Debug.Log("new connection UDP client: " + id);
            Server.instance.sendReceiveConnectionByUdp(id);
        }

        if (method.Equals("playerKeys")) {
            float x = packet.ReadFloat();
            float y = packet.ReadFloat();
            bool jumping = packet.ReadBool();
            Quaternion rotation = packet.ReadQuaternion();

            Server.instance.addAction(() => {
                Server.instance.playerKeys(id, x, y, jumping, rotation);
            });
        }
    }

    public void sendData(Packet packet, IPEndPoint endPoint) {
        try {
            if (endPoint == null) return;
            udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
        } catch {
            Debug.Log("Err. sending udp to client!");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Server : MonoBehaviour {
    public static Server instance = null;
    private TcpListener tcpListener;
    private UdpClient udpListener;
    private List<Client> clients = new List<Client>();
    private int maxPlayers = 50;
    private int players = 0;
    private int port = 8080;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        startServer();
    }

    private void startServer() {
        Debug.Log("Starting server...");

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new System.AsyncCallback(tcpConnectCallback), null);

        Debug.Log("Server started!");
    }

    private void tcpConnectCallback(System.IAsyncResult result) {
        TcpClient socket = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new System.AsyncCallback(tcpConnectCallback), null);
        
        if(players >= maxPlayers) {
            Debug.Log("Server is full.");
            return;
        }

        Debug.Log("New client tcp connected!");

        Client client = new Client(players++);
        Tcp tcp = new Tcp(socket);
        client.setTcp(tcp);
        client.getTcp().connect();

        udpListener = new UdpClient(port);
        Udp udp = new Udp(udpListener);
        client.setUdp(udp);
        client.getUdp().connect();
        
        clients.Add(client);
        sendTcpData(client.getId(), "Hello from server TCP.");
    }

    public void sendReceiveConnectionByUdp(int client) {
        sendUdpData(client, "Hello from server UDP.");
    }

    private void sendTcpData(int client, string msg) {
        Packet packet = new Packet();
        packet.Write(msg);
        packet.Write(client);
        packet.WriteLength();
        Debug.Log("Sending message to client.. " + msg);
        clients[client].sendTcpData(packet);
    }

    private void sendTcpDataToAll(string msg) {
        clients.ForEach(client => {
            Packet packet = new Packet();
            packet.Write(msg);
            packet.Write(client.getId());
            packet.WriteLength();
            Debug.Log("Sending message to clients.. " + msg);
            client.sendTcpData(packet);
        });
    }

    private void sendTcpDataToAll(int exceptClient, string msg) {
        clients.ForEach(client => {
            if(client.getId() != exceptClient) {
                Packet packet = new Packet();
                packet.Write(msg);
                packet.Write(client.getId());
                packet.WriteLength();
                Debug.Log("Sending message to clients.. " + msg);
                client.sendTcpData(packet);
            }
        });
    }

    private void sendUdpData(int client, string msg) {
        Packet packet = new Packet();
        packet.Write(msg);
        packet.Write(client);
        packet.WriteLength();
        Debug.Log("Sending message to client.. " + msg);
        clients[client].sendUdpData(packet);
    }

    private void sendUdpDataToAll(string msg) {
        clients.ForEach(client => {
            Packet packet = new Packet();
            packet.Write(msg);
            packet.Write(client.getId());
            packet.WriteLength();
            Debug.Log("Sending message to clients.. " + msg);
            client.sendUdpData(packet);
        });
    }

    private void sendUdpDataToAll(int exceptClient, string msg) {
        clients.ForEach(client => {
            if(client.getId() != exceptClient) {
                Packet packet = new Packet();
                packet.Write(msg);
                packet.Write(client.getId());
                packet.WriteLength();
                Debug.Log("Sending message to clients.. " + msg);
                client.sendUdpData(packet);
            }
        });
    }
}

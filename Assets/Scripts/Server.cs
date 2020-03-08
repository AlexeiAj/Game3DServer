using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Server : MonoBehaviour {
    public GameObject playerPrefab;
    public static Server instance = null;
    private TcpListener tcpListener;
    private UdpClient udpListener;
    private List<Client> clients = new List<Client>();
    private int maxPlayers = 50;
    private int totalPlayers = 0;
    private int port = 8080;
    private Udp udp;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        startServer();
    }

    private void FixedUpdate() {
        clients.ForEach(client => {
            GameObject player = client.getPlayer();
            
            if(player != null){
                client.setPosition(player.transform.position);
                client.setRotation(player.transform.rotation);
                client.setCamRotation(player.GetComponent<PlayerController>().camRotation);

                Packet packet = new Packet();
                packet.Write("playerPosition");
                packet.Write(client.getId());
                packet.Write(client.getPosition());
                packet.Write(client.getRotation());
                packet.Write(client.getCamRotation());

                sendUdpDataToAll(packet);
            }
        });
    }

    public void instantiatePlayer(int id, string username, Vector3 position, Quaternion rotation) {
        GameObject playerGO = Instantiate(playerPrefab, position, rotation);
        getClientById(id).setPlayer(playerGO);
    }

    private void startServer() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Debug.Log("Starting server...");

        udpListener = new UdpClient(port);
        udp = new Udp(udpListener);
        udp.connect();

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new System.AsyncCallback(tcpConnectCallback), null);

        Debug.Log("Server started!");
    }

    private void tcpConnectCallback(System.IAsyncResult result) {
        TcpClient socket = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new System.AsyncCallback(tcpConnectCallback), null);
        
        if(totalPlayers >= maxPlayers) {
            Debug.Log("Server is full.");
            return;
        }

        var rand = new System.Random();
        int x = rand.Next(-16, 16);
        int z = rand.Next(-16, 16);

        Client client = new Client(totalPlayers++, new Vector3(x, 5, z), Quaternion.identity);
        Tcp tcp = new Tcp(socket);
        client.setTcp(tcp);
        client.getTcp().connect();

        addClients(client);
        spawnPlayer(client.getId(), client.getPosition(), client.getRotation());

        Debug.Log("New client connected!");
    }

    public void addClients(Client client) {
        clients.Add(client);
    }

    private void sendTcpData(int id, Packet packet) {
        packet.WriteLength();
        getClientById(id).sendTcpData(packet);
    }

    private void sendTcpDataToAll(Packet packet) {
        packet.WriteLength();
        clients.ForEach(client => {
            client.sendTcpData(packet);
        });
    }

    private void sendTcpDataToAll(int exceptClient, Packet packet) {
        packet.WriteLength();
        clients.ForEach(client => {
            if(client.getId() != exceptClient) {
                client.sendTcpData(packet);
            }
        });
    }

    public void sendReceiveConnectionByUdp(int id) {
        Packet packet = new Packet();
        packet.Write("newConnectionUDP");
        packet.Write(id);
        sendUdpData(id, packet);
    }

    private void sendUdpData(int id, Packet packet) {
        packet.WriteLength();
        sendUdpData(packet, getClientById(id).getEndPointUdp());
    }

    private void sendUdpDataToAll(Packet packet) {
        packet.WriteLength();
        clients.ForEach(client => {
            sendUdpData(packet, client.getEndPointUdp());
        });
    }

    private void sendUdpDataToAll(int exceptClient, Packet packet) {
        packet.WriteLength();
        clients.ForEach(client => {
            if(client.getId() != exceptClient) {
                sendUdpData(packet, client.getEndPointUdp());
            }
        });
    }

    public void sendUdpData(Packet packet, IPEndPoint endPoint) {
        udp.sendData(packet, endPoint);
    }
    
    private void spawnPlayer(int id, Vector3 position, Quaternion rotation) {
        Packet packet = new Packet();
        packet.Write("spawnPlayer");
        packet.Write(id);
        packet.Write(position);
        packet.Write(rotation);

        sendTcpData(id, packet);

        clients.ForEach(client => {
            if(client.getId() != id) {
                packet = new Packet();
                packet.Write("newConnection");
                packet.Write(client.getId());
                packet.Write(client.getUsername());
                packet.Write(client.getPosition());
                packet.Write(client.getRotation());

                sendTcpData(id, packet);
            }
        });
    }

    public void newConnection(int id, string username) {
        Client client = getClientById(id);
        client.setUsername(username);

        Packet packet = new Packet();
        packet.Write("newConnection");
        packet.Write(client.getId());
        packet.Write(client.getUsername());
        packet.Write(client.getPosition());
        packet.Write(client.getRotation());

        sendTcpDataToAll(id, packet);

        ThreadManager.ExecuteOnMainThread(() => {
            Server.instance.instantiatePlayer(client.getId(), client.getUsername(), client.getPosition(), client.getRotation());
        });
    }

    public void playerKeys(int id, float x, float y, float mouseX, float mouseY, bool mouseLeft, bool mouseRight, bool jumping, bool shift, bool e) {
        GameObject player = getClientById(id).getPlayer();
        if(player == null) return;
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.setKeys(x, y, mouseX, mouseY, mouseLeft, mouseRight, jumping, shift, e);
    }

    public Client getClientById(int id) {
        return clients[id];
    }
}

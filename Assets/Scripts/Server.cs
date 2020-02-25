using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Server : MonoBehaviour {
    public static Server instance = null;
    private TcpListener tcpListener;
    private Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private int maxPlayers;
    private int port;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        startServer(50, 8080);
    }

    private void startServer(int maxPlayers, int port) {
        this.maxPlayers = maxPlayers;
        this.port = port;

        Debug.Log("Server starting");
        initializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, this.port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new System.AsyncCallback(tcpConnectCallback), null);

        Debug.Log("Server started");
    }

    private void tcpConnectCallback(System.IAsyncResult result) {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new System.AsyncCallback(tcpConnectCallback), null);

        Debug.Log("Conectando ao client!");

        for (int i = 1; i <= maxPlayers; i++) {
            Tcp tcp = clients[i].getTcp();
            int id = clients[i].getId();

            if(tcp.getSocket() == null){
                tcp.connect(client);
                sendMsg(id, "Bem vindo client!");
                return;
            }
        }

        Debug.Log("Server esta cheio!");
    }

    private void initializeServerData() {
        for (int i = 1; i <= maxPlayers; i++) clients.Add(i, new Client(i));
    }

    private void sendMsg(int client, string msg) {
        Packet packet = new Packet();
        packet.Write(msg);
        packet.Write(client);

        sendTcpData(client, packet);
    }

    private void sendTcpData(int client, Packet packet) {
        packet.WriteLength();
        clients[client].getTcp().sendData(packet);
    }

    private void sendTcpDataToAll(Packet packet) {
        packet.WriteLength();

        for (int i = 1; i <= maxPlayers; i++) {
            clients[i].getTcp().sendData(packet);
        }
    }

    private void sendTcpDataToAll(int exceptClient, Packet packet) {
        packet.WriteLength();

        for (int i = 1; i <= maxPlayers; i++) {
            if(i == exceptClient) continue;
            clients[i].getTcp().sendData(packet);
        }
    }
}

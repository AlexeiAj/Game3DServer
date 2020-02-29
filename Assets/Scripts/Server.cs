using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Server : MonoBehaviour {
    public static Server instance = null;
    private TcpListener tcpListener;
    // private UdpClient udpListener;
    private List<Client> clients = new List<Client>();
    private int maxPlayers;
    private int players = 0;
    private int port;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;

        this.maxPlayers = 50;
        this.port = 8080;
        startServer();
    }

    private void startServer() {
        Debug.Log("Starting server...");

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new System.AsyncCallback(tcpConnectCallback), null);

        // udpListener = new UdpClient(port);
        // udpListener.BeginReceive(udpReceiveCallback, null);

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
        
        clients.Add(client);
        sendTcpData(client.getId(), "Hello from server.");
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

//     private void udpReceiveCallback(System.IAsyncResult result) {
//         try {
//             IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
//             byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
//             udpListener.BeginReceive(udpReceiveCallback, null);

//             if (data.Length < 4) {
//                 Debug.Log("Desconectar udp");
//                 return;
//             }

//             Packet packet = new Packet(data);
//             int id = packet.ReadInt();
//             if (id == 0) return;

//             if(clients[id].getUdp().getEndPoint() == null) {
//                 clients[id].getUdp().setEndPoint(clientEndPoint);
//                 return;
//             }

//             // sendMsgUdp(id, "Bem vindo client udp!");

//             if (clients[id].getUdp().getEndPoint().ToString() == clientEndPoint.ToString()) {
//                 clients[id].getUdp().handleData(packet);
//             }
//         } catch (System.Exception e) {
//             Debug.Log(e);
//             Debug.Log("Desconectar udp");
//         }
//     }

//     private void sendMsgUdp(int client, string msg) {
//         Packet packet = new Packet();
//         packet.Write(msg);
//         packet.Write(client);

//         sendUdpData(client, packet);
//     }

//     private void sendUdpData(int client, Packet packet) {
//         packet.WriteLength();
//         IPEndPoint endPoint = clients[client].getUdp().getEndPoint();

//         try {
//             if (endPoint == null) return;
//             udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
//         } catch (System.Exception e) {
//             Debug.Log(e);
//             Debug.Log("Erro ao enviar data para client udp");
//         }
//     }

//     private void sendUdpDataToAll(Packet packet) {
//         packet.WriteLength();

//         for (int i = 1; i <= maxPlayers; i++) {
//             IPEndPoint endPoint = clients[i].getUdp().getEndPoint();

//             try {
//                 if (endPoint != null) {
//                     udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
//                 }
//             } catch (System.Exception e) {
//                 Debug.Log(e);
//                 Debug.Log("Erro ao enviar data para client udp");
//             }
//         }
//     }

//     private void sendUdpDataToAll(int exceptClient, Packet packet) {
//         packet.WriteLength();

//         for (int i = 1; i <= maxPlayers; i++) {
//             if(i == exceptClient) continue;

//             IPEndPoint endPoint = clients[i].getUdp().getEndPoint();

//             try {
//                 if (endPoint != null) {
//                     udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
//                 }
//             } catch (System.Exception e) {
//                 Debug.Log(e);
//                 Debug.Log("Erro ao enviar data para client udp");
//             }
//         }
//     }
}

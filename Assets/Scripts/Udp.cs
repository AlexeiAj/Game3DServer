// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Net;
// using System.Net.Sockets;

// public class Udp {
//     private IPEndPoint endPoint;

//     public void handleData(Packet packetData) {
//         int packetLenght = packetData.ReadInt();
//         byte[] packetBytes = packetData.ReadBytes(packetLenght);

//         Packet packet = new Packet(packetBytes);
        
//         string msg = packet.ReadString();
//         string username = packet.ReadString();
//         int id = packet.ReadInt();
//         Debug.Log("msgUDP: " + msg + " id: " + id + " username: " + username);
//     }
    
//     public IPEndPoint getEndPoint(){
//         return endPoint;
//     }
    
//     public void setEndPoint(IPEndPoint endPoint) {
//         this.endPoint = endPoint;
//     }
// }

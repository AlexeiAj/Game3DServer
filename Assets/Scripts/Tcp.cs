﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Tcp {
    private int dataBufferSize = 4096;
    private TcpClient socket;
    private NetworkStream stream;
    private byte[] recieveBuffer;
    private Packet receiveData;

    public Tcp (TcpClient socket) {
        this.socket = socket;
    }

    public void connect() {
        socket.ReceiveBufferSize = dataBufferSize;
        socket.SendBufferSize = dataBufferSize;

        stream = socket.GetStream();

        receiveData = new Packet();
        recieveBuffer = new byte[dataBufferSize];

        stream.BeginRead(recieveBuffer, 0, dataBufferSize, receiveCallback, null);
    }

    private void receiveCallback(System.IAsyncResult result) {
        try {
            int byteLenght = stream.EndRead(result);

            if (byteLenght <= 0) {
                Debug.Log("Disconnecting client tcp...");
                return;
            }

            byte[] data = new byte[byteLenght];
            System.Array.Copy(recieveBuffer, data, byteLenght);
            receiveData.Reset(handleData(data));

            stream.BeginRead(recieveBuffer, 0, dataBufferSize, receiveCallback, null);
        } catch (System.Exception e) {
            Debug.Log(e);
            Debug.Log("Disconnecting client tcp...");
        }
    }

    private bool handleData(byte[] data) {
        int packetLenght = 0;

        receiveData.SetBytes(data);

        if (receiveData.UnreadLength() >= 4) {
            packetLenght = receiveData.ReadInt();
            if (packetLenght <= 0) return true;
        }

        while(packetLenght > 0 && packetLenght <= receiveData.UnreadLength()) {
            byte[] packetBytes = receiveData.ReadBytes(packetLenght);
            Packet packet = new Packet(packetBytes);

            string msg = packet.ReadString();
            string username = packet.ReadString();
            int id = packet.ReadInt();

            Debug.Log("Client tcp message: " + msg + " id: " + id + " username: " + username);

            packetLenght = 0;

            if (receiveData.UnreadLength() >= 4) {
                packetLenght = receiveData.ReadInt();
                if (packetLenght <= 0) return true;
            }
        }

        return packetLenght <= 1;
    }

    public void sendData(Packet packet) {
        try {
            if (socket == null) return;
            stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
        } catch {
            Debug.Log("Err. sending tcp to client!");
        }
    }
}

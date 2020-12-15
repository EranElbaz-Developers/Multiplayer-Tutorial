using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Unity.Mathematics;
using UnityEngine;


public class Server : MonoBehaviour
{
    private UserManager um;
    private Dictionary<int, GameObject> clientGos;

    private struct UdpHelper
    {
        public UdpClient client;
        public IPEndPoint endPoint;
    }
    
    
    private void Awake()
    {
        um = new UserManager();
        clientGos = new Dictionary<int, GameObject>();
        TcpServer();
        UdpServer();
    }

    void UdpServer()
    {
        Debug.Log($"Starting UDP server on port {Utils.UDP_PORT}");
        var udpHelper = new UdpHelper();
        udpHelper.client = new UdpClient(Utils.UDP_PORT);
        udpHelper.endPoint = new IPEndPoint(IPAddress.Any, Utils.UDP_PORT);
        udpHelper.client.BeginReceive(UdpReceived, udpHelper);
    }


    void UdpReceived(IAsyncResult ar)
    {
        IPEndPoint senderIP = new IPEndPoint(IPAddress.Any, 0);
        var client = ((UdpHelper) (ar.AsyncState)).client;
        client.BeginReceive(UdpReceived, ar.AsyncState);

        var payload = client.EndReceive(ar, ref senderIP);
        var dataJson = Encoding.ASCII.GetString(payload);
        var playerData = JsonUtility.FromJson<PlayerDataPacket>(dataJson);

        if (um.users[playerData.clientId].lastPacketCounter < playerData.packetCounter)
        {
            Debug.Log($"Got new data for user {playerData.clientId}");
            var data = um.users[playerData.clientId];
            data.position = playerData.position;
            data.rotation = playerData.rotation;
            data.lastPacketCounter = playerData.packetCounter;
        }
        
    }
    
    
    void TcpServer()
    {
        Debug.Log($"Starting TCP server on port: {Utils.TCP_PORT}");

        TcpListener listener = new TcpListener(IPAddress.Any, Utils.TCP_PORT);
        listener.Start();
        listener.BeginAcceptTcpClient(TcpReceived, listener);
    }

    private void TcpReceived(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener) ar.AsyncState;
        listener.BeginAcceptTcpClient(TcpReceived, ar.AsyncState);

        using (TcpClient client = listener.EndAcceptTcpClient(ar))
        using (var nwStream = client.GetStream())
        {
            var payload = Utils.ReadData(nwStream);
            if (payload.Length > 0)
            {
                var requestJson = Encoding.ASCII.GetString(payload);
                var loginRequest = JsonUtility.FromJson<LoginRequest>(requestJson);

                var userId = um.users.Count;

                var pds = new PlayerDataServer(Vector3.zero, Vector3.zero, loginRequest.username, userId, 0);
                
                um.users.Add(userId, pds);
                Debug.Log($"New user login {userId}, with nickname {loginRequest.username}");

                var response = new LoginResponse(userId);
                var responseJson = JsonUtility.ToJson(response);
                var responsePayload = Encoding.ASCII.GetBytes(responseJson);
                nwStream.Write(responsePayload, 0, responsePayload.Length);
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (var user in um.users)
        {
            GameObject clientGo;
            if (!clientGos.TryGetValue(user.Key, out clientGo))
            {
                clientGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                clientGo.name = user.Value.name;
                clientGo.transform.parent = transform;
                clientGos.Add(user.Key, clientGo);
            }

            var clientTransform = clientGo.transform;
            clientTransform.position = user.Value.position;
            clientTransform.rotation = quaternion.Euler(user.Value.rotation);
        }
    }
}





















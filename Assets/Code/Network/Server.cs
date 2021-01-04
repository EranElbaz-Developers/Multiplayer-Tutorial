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

    private void Awake()
    {
        um = new UserManager();
        clientGos = new Dictionary<int, GameObject>();
        TcpServer(Utils.TCP_PORT);
        var udp = new UdpServer(UdpReceived);
    }

    void UdpReceived(byte[] payload)
    {
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

    void TcpServer(int port)
    {
        Debug.Log($"Starting TCP server on port: {port}");

        TcpListener listener = new TcpListener(IPAddress.Any, port);
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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class Server : MonoBehaviour
{
    public Dictionary<int, ClientDataServer> clients;
    private Dictionary<int, GameObject> clientGos;

    private void Awake()
    {
        clients = new Dictionary<int, ClientDataServer>();
        clientGos = new Dictionary<int, GameObject>();
        var tcp = new TcpServer(TcpReceived);
        var udp = new UdpServer(UdpReceived);
        tcp.Start(Utils.SERVER_TCP_PORT);
        udp.Start(Utils.SERVER_UDP_PORT);
    }

    void UdpReceived(string dataJson)
    {
        var playerData = JsonUtility.FromJson<PlayerDataPacket>(dataJson);

        if (clients[playerData.clientId].lastPacketCounter < playerData.packetCounter)
        {
            Debug.Log($"Got new data for user {playerData.clientId}");
            var data = clients[playerData.clientId];
            data.clientTransform = playerData.clientTransform;
            data.lastPacketCounter = playerData.packetCounter;
        }
    }

    private byte[] TcpReceived(string requestJson)
    {
        var loginRequest = JsonUtility.FromJson<LoginRequest>(requestJson);

        var clientId = clients.Count;

        var pds = new ClientDataServer(new ClientTransform(Vector3.zero, Quaternion.identity), loginRequest.clientName,
            clientId,
            0, loginRequest.clientIp);

        clients.Add(clientId, pds);
        Debug.Log($"New client login {clientId}, with nickname {loginRequest.clientName}");

        var response = new LoginResponse(clientId);
        var responseJson = JsonUtility.ToJson(response);
        return Encoding.ASCII.GetBytes(responseJson); 
    }

    private void FixedUpdate()
    {
        var clientTransforms = new Dictionary<int, ClientTransform>();
        foreach (var client in clients)
        {
            GameObject clientGo;
            if (!clientGos.TryGetValue(client.Key, out clientGo))
            {
                clientGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                clientGo.name = client.Value.name;
                clientGo.transform.parent = transform;
                clientGos.Add(client.Key, clientGo);
            }

            var clientGoTransform = clientGo.transform;
            clientGoTransform.position = client.Value.clientTransform.position;
            clientGoTransform.rotation = client.Value.clientTransform.rotation;
            clientTransforms.Add(client.Key, client.Value.clientTransform);
        }

        using (UdpClient udpClient = new UdpClient())
        {
            foreach (var client in clients)
            {
                var payloadJson = JsonUtility.ToJson(clientTransforms);
                var payload = Encoding.ASCII.GetBytes(payloadJson);
                udpClient.Send(payload, payload.Length, client.Value.clientIp, Utils.CLIENT_UDP_PORT);
            }
        }
    }
}
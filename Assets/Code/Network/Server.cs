using System.Collections.Generic;
using System.Text;
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
        var tcp = new TcpServer(TcpReceived);
        var udp = new UdpServer(UdpReceived);
        tcp.Start(Utils.SERVER_TCP_PORT);
        udp.Start(Utils.SERVER_UDP_PORT);
    }

    void UdpReceived(string dataJson)
    {
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

    private byte[] TcpReceived(string requestJson)
    {
        var loginRequest = JsonUtility.FromJson<LoginRequest>(requestJson);

        var userId = um.users.Count;

        var pds = new PlayerDataServer(Vector3.zero, Vector3.zero, loginRequest.username, userId, 0);

        um.users.Add(userId, pds);
        Debug.Log($"New user login {userId}, with nickname {loginRequest.username}");

        var response = new LoginResponse(userId);
        var responseJson = JsonUtility.ToJson(response);
        return Encoding.ASCII.GetBytes(responseJson);
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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class Client : MonoBehaviour
{
    public InputField userInput;
    public string server;
    public GameObject player;
    public GameObject loginForm;
    private int id;
    private TcpClient tcpClient;
    private UdpClient udpClient;
    private int udpCounter;
    private UdpServer udpServer;
    private Dictionary<int, ClientTransform> otherClientsData;
    private Dictionary<int, GameObject> otherClientsObjects;

    private void Awake()
    {
        tcpClient = new TcpClient(server, Utils.SERVER_TCP_PORT);
        udpClient = new UdpClient();
        udpCounter = 0;
        udpServer = new UdpServer(UdpReceived);
        otherClientsData = new Dictionary<int, ClientTransform>();
        otherClientsObjects = new Dictionary<int, GameObject>();
    }

    private void UdpReceived(string payload)
    {
        otherClientsData = JsonConvert.DeserializeObject<Dictionary<int, ClientTransform>>(payload);
    }

    private void OnDestroy()
    {
        tcpClient.Close();
        udpClient.Close();
    }

    public void OnLoginPress()
    {
        using (var nwStream = tcpClient.GetStream())
        {
            var request = new LoginRequest(userInput.text);
            var jsonRequest = JsonUtility.ToJson(request);
            var requestPayload = Encoding.ASCII.GetBytes(jsonRequest);
            nwStream.Write(requestPayload, 0, requestPayload.Length);

            var responsePayload = Utils.ReadData(nwStream);
            var jsonResponse = Encoding.ASCII.GetString(responsePayload);
            var response = JsonUtility.FromJson<LoginResponse>(jsonResponse);

            id = response.id;
            player.SetActive(true);
            loginForm.SetActive(false);
            udpServer.Start(Utils.CLIENT_UDP_PORT);
        }
    }

    private void FixedUpdate()
    {
        var playerData = new PlayerDataPacket(new ClientTransform(player.transform.position, player.transform.rotation),
            id,
            udpCounter);
        udpCounter++;
        var jsonRequest = JsonUtility.ToJson(playerData);
        var requestPayload = Encoding.ASCII.GetBytes(jsonRequest);
        udpClient.Send(requestPayload, requestPayload.Length, server, Utils.SERVER_UDP_PORT);
        
        
        foreach (var clientData in otherClientsData)
        {
            if (clientData.Key != id)
            {
                GameObject clientGo;
                if (!otherClientsObjects.TryGetValue(clientData.Key, out clientGo))
                {
                    clientGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    clientGo.transform.parent = transform;
                    otherClientsObjects.Add(clientData.Key, clientGo);
                }

                var clientGoTransform = clientGo.transform;
                clientGoTransform.position = clientData.Value.position;
                clientGoTransform.rotation = clientData.Value.rotation;
            }
        }
    }
}
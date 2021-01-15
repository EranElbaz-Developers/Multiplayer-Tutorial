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

    private void Awake()
    {
        tcpClient = new TcpClient(server, Utils.SERVER_TCP_PORT);
        udpClient = new UdpClient();
        udpCounter = 0;
        udpServer = new UdpServer(UdpReceived);
    }

    private void UdpReceived(string payload)
    {
        Debug.Log(payload);
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
    }
}
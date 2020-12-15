using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        tcpClient = new TcpClient(server, Utils.TCP_PORT);
        udpClient = new UdpClient(server, Utils.UDP_PORT);
        udpCounter = 0;
    }

    private void OnDestroy()
    {
        tcpClient.Close();
        udpClient.Close();
    }

    public void OnLoginPress()
    {
        using (TcpClient client = new TcpClient(server, Utils.TCP_PORT))
        using (var nwStream = client.GetStream())
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
        }
    }

    private void FixedUpdate()
    {
        var playerData = new PlayerDataPacket(player.transform.position, player.transform.rotation.eulerAngles, id,
            udpCounter);
        udpCounter++;
        var jsonRequest = JsonUtility.ToJson(playerData);
        var requestPayload = Encoding.ASCII.GetBytes(jsonRequest);
        udpClient.Send(requestPayload, requestPayload.Length);
    }
}
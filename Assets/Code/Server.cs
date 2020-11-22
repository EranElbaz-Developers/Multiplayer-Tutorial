using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class Server : MonoBehaviour
{
    private UserManager um;

    private void Awake()
    {
        um = new UserManager();
        TcpServer();
    }

    void TcpServer()
    {
        Debug.Log($"Starting TCP server on port: {Utils.PORT}");

        TcpListener listener = new TcpListener(IPAddress.Any, Utils.PORT);
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
                um.users.Add(userId, loginRequest.username);
                Debug.Log($"New user login {userId}, with nickname {loginRequest.username}");

                var response = new LoginResponse(userId);
                var responseJson = JsonUtility.ToJson(response);
                var responsePayload = Encoding.ASCII.GetBytes(responseJson);
                nwStream.Write(responsePayload, 0, responsePayload.Length);
            }
        }
    }
}
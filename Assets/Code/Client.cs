using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public InputField userInput;
    public string server;

    public void OnLoginPress()
    {
        using (TcpClient client = new TcpClient(server, Utils.PORT))
        using (var nwStream = client.GetStream())
        {
            var request = new LoginRequest(userInput.text);
            var jsonRequest = JsonUtility.ToJson(request);
            var requestPayload = Encoding.ASCII.GetBytes(jsonRequest);
            nwStream.Write(requestPayload, 0, requestPayload.Length);

            var responsePayload = Utils.ReadData(nwStream);
            var jsonResponse = Encoding.ASCII.GetString(responsePayload);
            var response = JsonUtility.FromJson<LoginResponse>(jsonResponse);
            
            Debug.Log(response.id);
            
        }
    }
}
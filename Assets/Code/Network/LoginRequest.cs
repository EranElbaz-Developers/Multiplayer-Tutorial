using System;
using System.Net;
using System.Net.Sockets;

public class LoginRequest
{
    public string clientName;
    public string clientIp;

    public LoginRequest(string clientName)
    {
        this.clientName = clientName;
        clientIp = GetLocalIPAddress();
    }

    static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}
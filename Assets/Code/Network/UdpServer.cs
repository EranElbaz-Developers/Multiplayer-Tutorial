using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpServer
{
    public delegate void AfterReceiveFunction(byte[] payload);

    public AfterReceiveFunction func;

    struct UdpHelper
    {
        public UdpClient client;
        public IPEndPoint endPoint;
    }
    
    public UdpServer(AfterReceiveFunction func)
    {
        this.func = func;
    }
    
    void Start(int port)
    {
        Debug.Log($"Starting UDP server on port {port}");
        var udpHelper = new UdpHelper();
        udpHelper.client = new UdpClient(port);
        udpHelper.endPoint = new IPEndPoint(IPAddress.Any, port);
        udpHelper.client.BeginReceive(UdpReceived, udpHelper);
    }

    void UdpReceived(IAsyncResult ar)
    {
        IPEndPoint senderIP = new IPEndPoint(IPAddress.Any, 0);
        var client = ((UdpHelper) (ar.AsyncState)).client;
        client.BeginReceive(UdpReceived, ar.AsyncState);

        var payload = client.EndReceive(ar, ref senderIP);
        func(payload);
    }
}
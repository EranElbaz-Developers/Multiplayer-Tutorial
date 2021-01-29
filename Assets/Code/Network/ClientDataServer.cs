public class ClientDataServer
{
    public ClientTransform clientTransform;
    public string name;
    public int clientId;
    public int lastPacketCounter;
    public string clientIp;

    public ClientDataServer(ClientTransform clientTransform, string name, int clientId, int lastPacketCounter, string clientIp)
    {
        this.clientTransform = clientTransform;
        this.name = name;
        this.clientId = clientId;
        this.lastPacketCounter = lastPacketCounter;
        this.clientIp = clientIp;
    }
}
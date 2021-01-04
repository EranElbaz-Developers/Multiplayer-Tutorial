public class PlayerDataPacket
{
    public ClientTransform clientTransform;
    public int clientId;
    public int packetCounter;

    public PlayerDataPacket(ClientTransform clientTransform, int clientId, int packetCounter)
    {
        this.clientTransform = clientTransform;
        this.clientId = clientId;
        this.packetCounter = packetCounter;
    }
}
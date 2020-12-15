using UnityEngine;

public class PlayerDataPacket
{
    public Vector3 position;
    public Vector3 rotation;
    public int clientId;
    public int packetCounter;

    public PlayerDataPacket(Vector3 position, Vector3 rotation, int clientId, int packetCounter)
    {
        this.position = position;
        this.rotation = rotation;
        this.clientId = clientId;
        this.packetCounter = packetCounter;
    }
}
using UnityEngine;

public class PlayerDataServer
{
    public Vector3 position;
    public Vector3 rotation;
    public string name;
    public int clientId;
    public int lastPacketCounter;

    public PlayerDataServer(Vector3 position, Vector3 rotation, string name, int clientId, int lastPacketCounter)
    {
        this.position = position;
        this.rotation = rotation;
        this.name = name;
        this.clientId = clientId;
        this.lastPacketCounter = lastPacketCounter;
    }
}
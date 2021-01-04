using UnityEngine;

public class ClientTransform
{
    public Vector3 position;
    public Quaternion rotation;

    public ClientTransform(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct ConnectionPoint
{
    [SerializeField]
    public Vector2 Position;
    [SerializeField]
    public Vector2 Rotation;

    public ConnectionPoint(Vector2 position, Vector2 rotation)
    {
        Position = position;
        Rotation = rotation;
    }
    public ConnectionPoint(ConnectionPoint existingConnection)
    {
        Position = existingConnection.Position;
        Rotation = existingConnection.Rotation;
    }
    public static bool operator ==(ConnectionPoint connectionA, ConnectionPoint connectionB)
    {
        return connectionA.Equals(connectionB);
    }
    public static bool operator !=(ConnectionPoint connectionA, ConnectionPoint connectionB)
    {
        return !connectionA.Equals(connectionB);
    }

    public void Print() {
        Debug.Log("Connection Point: " + Position + " Direction " + Rotation);
    }
    public ConnectionPoint GetCopy() {
        return new ConnectionPoint(Position, Rotation);
    }
}

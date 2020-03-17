using UnityEngine;

[System.Serializable]
public struct Connection
{
    public Vector3 Position;
    public Vector3 Direction;
    public Connection(Vector3 pos, Vector3 dir) {
        Position = pos;
        Direction = dir;
    }
}

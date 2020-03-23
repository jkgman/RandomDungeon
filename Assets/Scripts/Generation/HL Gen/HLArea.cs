using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HLArea
{
    public HLAreaData data;
    private List<Connection> connections = new List<Connection>();
    private List<Connection> backConnection = new List<Connection>();
    private List<ConnectionPoint> connectionPoints;
    private List<ConnectionPoint> unusedConnection;
    public Rect rect;

    public List<Connection> Connections { get => connections; private set => connections = value; }
    public List<Connection> BackConnection { get => backConnection; private set => backConnection = value; }
    public List<ConnectionPoint> UnusedConnection { get => unusedConnection; }
    public List<ConnectionPoint> ConnectionPoints { get => connectionPoints;  }

    public HLArea(HLAreaData data, Vector2 rectPos) {
        this.data = data;
        unusedConnection = data.ConnectionsPoints;
        connectionPoints = data.ConnectionsPoints;
        SetRect(rectPos, data.RectSize);
    }

    public void Rotate(float rot) {
        for (int i = 0; i < UnusedConnection.Count; i++)
        {
            UnusedConnection[i] = new ConnectionPoint(Vector2Extensions.RotateDegree(UnusedConnection[i].Position, rot), Vector2Extensions.RotateDegree(UnusedConnection[i].Rotation, rot));
        }
        for (int i = 0; i < ConnectionPoints.Count; i++)
        {
            ConnectionPoints[i] = new ConnectionPoint(Vector2Extensions.RotateDegree(ConnectionPoints[i].Position, rot), Vector2Extensions.RotateDegree(ConnectionPoints[i].Rotation, rot));

        }
        rect = new Rect(rect.position, Vector2Extensions.RotateDegree(rect.size, rot));
    }
    public void SetRectPosition(Vector2 rectPos) {
        rect = new Rect(rectPos, rect.size);
    }

    private void SetRect(Vector2 rectPos, Vector2 rectSize) {
        rect = new Rect(rectPos.x - rectSize.x/2, rectPos.y - rectSize.y/2, rectSize.x, rectSize.y);
    }
    public Vector2 GetRectCenter()
    {
        return rect.position - rect.size / 2;
    }

    public bool MaxConnectionsReached() {
        return (connections.Count + backConnection.Count >= data.ConnectionsPoints.Count);
    }
    public int GetConnectionIndex(ConnectionPoint point){
        for (int i = 0; i < connectionPoints.Count; i++)
        {
            if (connectionPoints[i] == point)
            {
                return i;
            }
        }
        return -1;
    }
    public void AddConnection(Connection connection, bool isBackwards) {
        for (int i = 0; i < unusedConnection.Count; i++)
        {
            if (unusedConnection[i] == data.ConnectionsPoints[connection.FromConnectionPoint])
            {
                unusedConnection.RemoveAt(i);
                break;
            }
            else if (i == unusedConnection.Count -1)
            {
                throw new System.Exception("Trying to add connection in none valid Direction");
            }
        }
        if (isBackwards)
        {
            BackConnection.Add(connection);
        }
        else
        {
            Connections.Add(connection);
        }
    }

    public ConnectionPoint FindValidDirection() {
        return unusedConnection[Random.Range(0, unusedConnection.Count)];
    }
}

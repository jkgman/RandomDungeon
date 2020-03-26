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
    private List<ConnectionPoint> unusedConnections;
    public Rect rect;

    public List<Connection> Connections { get => connections; private set => connections = value; }
    public List<Connection> BackConnection { get => backConnection; private set => backConnection = value; }
    public List<ConnectionPoint> UnusedConnections { get => unusedConnections; }
    public List<ConnectionPoint> ConnectionPoints { get => connectionPoints;  }

    public HLArea(HLAreaData data, Vector2 rectPos) {
        this.data = data;
        unusedConnections = data.CopyOfConnections();
        connectionPoints = data.CopyOfConnections();
        SetRect(rectPos, data.RectSize);
    }

    public HLArea(HLAreaData data)
    {
        this.data = data;
        unusedConnections = data.CopyOfConnections();
        connectionPoints = data.CopyOfConnections();
        SetRect(Vector2.zero, data.RectSize);
    }


    /*Get and Sets*/
    public Vector2 GetRectCenter()
    {
        return rect.position + (rect.size / 2);
    }

    public int GetConnectionIndex(ConnectionPoint point)
    {
        for (int i = 0; i < connectionPoints.Count; i++)
        {
            if (connectionPoints[i] == point)
            {
                return i;
            }
        }
        return -1;
    }

    private void SetRect(Vector2 rectPos, Vector2 rectSize)
    {
        rect = new Rect(rectPos.x - rectSize.x / 2, rectPos.y - rectSize.y / 2, rectSize.x, rectSize.y);
    }

    public void SetPosition(Vector2 rectPos)
    {
        rect = new Rect(rectPos, rect.size);
    }


    /*Evaluators*/
    public bool IsMaxConnectionsReached()
    {
        return (connections.Count + backConnection.Count >= data.ConnectionPoints.Count);
    }

    public ConnectionPoint FindValidDirection()
    {
        return unusedConnections[Random.Range(0, unusedConnections.Count)];
    }


    /*Mutators*/
    public void RotateAroundRectPos(float rot) {
        if (BackConnection.Count > 0 || Connections.Count > 0)
        {
            throw new System.Exception("Shouldn't rotate connected Areas");
        }
        for (int i = 0; i < UnusedConnections.Count; i++)
        {
            UnusedConnections[i] = new ConnectionPoint(Vector2Extensions.RotateDegree(UnusedConnections[i].Position, rot), Vector2Extensions.RotateDegree(UnusedConnections[i].Rotation, rot));
        }
        for (int i = 0; i < ConnectionPoints.Count; i++)
        {
            ConnectionPoints[i] = new ConnectionPoint(Vector2Extensions.RotateDegree(ConnectionPoints[i].Position, rot), Vector2Extensions.RotateDegree(ConnectionPoints[i].Rotation, rot));
        }
        rect = new Rect(rect.position, Vector2Extensions.RotateDegree(rect.size, rot));
    }

    public void RotateAroundRectCenter() {

    }
   
    public void AddConnection(Connection connection, bool isBackwards) {
        for (int i = 0; i < unusedConnections.Count; i++)
        {
            unusedConnections[i].Print();
            if (unusedConnections[i] == ConnectionPoints[connection.FromConnectionPoint])
            {
                unusedConnections.RemoveAt(i);
                break;
            }
            else if (i == unusedConnections.Count -1)
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

    /*Debuging*/
    public void PrintAreaSummary()
    {
        PrintAreaSummary(false, false, false, false);
    }
    public void PrintAreaSummary(bool PrintExtraInfo) {
        PrintAreaSummary(PrintExtraInfo, PrintExtraInfo, PrintExtraInfo, PrintExtraInfo);
    }
    public void PrintAreaSummary(bool PrintConnectionPoints, bool PrintConnections)
    {
        PrintAreaSummary(PrintConnectionPoints, PrintConnectionPoints, PrintConnections, PrintConnections);
    }
    public void PrintAreaSummary(bool showConnectionPoints, bool showUnusedConnectionPoints, bool showConnections, bool showBackConnections) {
        string summary = "HLArea: " + rect + " ";
        if (showConnectionPoints)
        {
            summary += "With Connection Points at:\n";
            for (int i = 0; i < ConnectionPoints.Count; i++)
            {
                summary += "Pos: " + ConnectionPoints[i].Position + " Rot: " + ConnectionPoints[i].Rotation + "\n";
            }
        }
        if (showUnusedConnectionPoints)
        {
            summary += "With Unused Connections:\n";
            for (int i = 0; i < UnusedConnections.Count; i++)
            {
                summary += "Pos: " + UnusedConnections[i].Position + " Rot: " + UnusedConnections[i].Rotation + "\n";
            }
        }
        if (showConnections)
        {
            summary += "With Connections:\n";
            for (int i = 0; i < Connections.Count; i++)
            {
                summary += "From Index: " + Connections[i].FromAreaIndex + " From Connection Index: " + Connections[i].FromConnectionPoint + " To Index: " + Connections[i].ToAreaIndex + " To Connection Index: " + Connections[i].ToConnectionPoint + "\n";
            }
        }
        if (showBackConnections)
        {
            summary += "With Back Connections:\n";
            for (int i = 0; i < BackConnection.Count; i++)
            {
                summary += "From Index: " + BackConnection[i].FromAreaIndex + " From Connection Index: " + BackConnection[i].FromConnectionPoint + " To Index: " + BackConnection[i].ToAreaIndex + " To Connection Index: " + BackConnection[i].ToConnectionPoint + "\n";
            }
        }
        Debug.Log(summary);
    }
}

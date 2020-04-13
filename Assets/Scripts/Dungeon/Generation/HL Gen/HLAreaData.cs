using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HLAreaData
{

    [SerializeField]
    Vector2 rectSize;
    [SerializeField]
    LLGenerator lLGenerator;
    [SerializeField]
    List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();


    public Vector2 RectSize { get => rectSize; }
    public LLGenerator LLGenerator { get => lLGenerator; }
    public List<ConnectionPoint> ConnectionPoints { get => connectionPoints; }

    public HLAreaData(Vector2 rectSize, LLGenerator lLGenerator, List<ConnectionPoint> connectionsPoints)
    {
        this.rectSize = rectSize;
        this.lLGenerator = lLGenerator;
        for (int i = 0; i < connectionsPoints.Count; i++)
        {
            this.connectionPoints.Add(new ConnectionPoint(connectionsPoints[i]));
        }
    }

    public HLAreaData GetCopy() {
        return new HLAreaData(rectSize, lLGenerator, connectionPoints);
    }
    public List<ConnectionPoint> CopyOfConnections() {
        List<ConnectionPoint> newList = new List<ConnectionPoint>();
        for (int i = 0; i < connectionPoints.Count; i++)
        {
            newList.Add(connectionPoints[i].GetCopy());
        }
        return newList;
    }

}

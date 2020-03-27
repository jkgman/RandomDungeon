using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HLAreaData
{

    [SerializeField]
    Vector2 rectSize;
    [SerializeField]
    GameObject island;
    [SerializeField]
    List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();

    //[SerializeField]
    //List<Loot> loot;
    //[SerializeField]
    //List<Enemies> enemies;


    public Vector2 RectSize { get => rectSize; }
    public GameObject Island { get => island; }
    public List<ConnectionPoint> ConnectionPoints { get => connectionPoints; }

    public HLAreaData(Vector2 rectSize, GameObject island, List<ConnectionPoint> connectionsPoints)
    {
        this.rectSize = rectSize;
        this.island = island;
        for (int i = 0; i < connectionsPoints.Count; i++)
        {
            this.connectionPoints.Add(new ConnectionPoint(connectionsPoints[i]));
        }
    }

    public HLAreaData GetCopy() {
        return new HLAreaData(rectSize, island, connectionPoints);
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

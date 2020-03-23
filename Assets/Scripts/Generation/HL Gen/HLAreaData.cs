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
    List<ConnectionPoint> connectionsPoints = new List<ConnectionPoint>();

    [SerializeField]
    List<Loot> loot;
    [SerializeField]
    List<Enemies> enemies;


    public Vector2 RectSize { get => rectSize; }
    public GameObject Island { get => island; }
    public List<ConnectionPoint> ConnectionsPoints { get => connectionsPoints; }

    public HLAreaData(Vector2 rectSize, GameObject island, List<ConnectionPoint> connectionsPoints, List<Loot> loot, List<Enemies> enemies)
    {
        this.rectSize = rectSize;
        this.island = island;
        for (int i = 0; i < connectionsPoints.Count; i++)
        {
            this.connectionsPoints.Add(new ConnectionPoint(connectionsPoints[i]));
        }
        this.loot = loot;
        this.enemies = enemies;
    }

    public HLAreaData GetCopy() {
        return new HLAreaData(rectSize, island, connectionsPoints, loot, enemies);
    }

}

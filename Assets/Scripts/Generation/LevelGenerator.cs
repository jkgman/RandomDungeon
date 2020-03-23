using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    float areaSize = 25;

    [SerializeField]
    SeedSettings seed;
    private HLMap _hlMap;
    
    private void Start()
    {
        Generate(seed);
        DisplayHLMap(_hlMap);
    }

    private void Generate(SeedSettings seed)
    {
        //pass seed to hlgen to return hl map
        _hlMap = HLGenerator.GenerateHighLevelMap(seed);
        
        //call hlmap to gen llmaps
        //pass final map to display
    }

    /// <summary>
    /// Display HLMAP
    /// </summary>
    /// <param name="map"></param>
    private void Display(HLMap map) {

    }

    private void DisplayHLMap(HLMap map)
    {
        Instantiate(map.areas[0].data.Island, new Vector3(map.areas[0].GetRectCenter().x,0, map.areas[0].GetRectCenter().y), Quaternion.identity, transform);
        //map.areas[0].Position = Vector3.zero;
        for (int i = 0; i < map.areas[0].Connections.Count; i++)
        {
            DisplayArea(map, 0, map.areas[0].Connections[i]);
        }
    }

    private void DisplayArea(HLMap map, int index, Connection connection)
    {
        //Vector3 pos = map.areas[index].Position + map.areas[index].data.ConnectionsPos[connection.FromConnectionPoint] + seed.BridgeLength;
        //Vector3 bridgePos = map.areas[index].Position + EvaluateToDirection(connection.Direction) / 2;
        Instantiate(map.areas[connection.ToAreaIndex].data.Island, new Vector3( map.areas[connection.ToAreaIndex].GetRectCenter().x,0, map.areas[connection.ToAreaIndex].GetRectCenter().y), Quaternion.identity, transform);
        //Instantiate(map.GetRandomBridge(), bridgePos, Quaternion.Euler(EvaluateToDirection(connection.Direction)), transform);
        for (int i = 0; i < map.areas[connection.ToAreaIndex].Connections.Count; i++)
        {
            DisplayArea(map, connection.ToAreaIndex, map.areas[connection.ToAreaIndex].Connections[i]);
        }
    }

}

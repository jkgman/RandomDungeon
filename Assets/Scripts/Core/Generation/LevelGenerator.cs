using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    SeedSettings seed;
    private HLMap _hlMap;
    
    private void Start()
    {
        Generate(seed);
        //DisplayHLMap(_hlMap);
    }

    private void Generate(SeedSettings seed)
    {
        //pass seed to hlgen to return hl map
        _hlMap = HLGenerator.GenerateHighLevelMap(seed);
        _hlMap.GenerateLL();
        //pass final map to display
    }


    private void PrintMapData(HLMap map) {
        for (int i = 0; i < map.areas.Count; i++)
        {
            Debug.Log("Size " + map.areas[i].rect.size + " Pos " + map.areas[i].rect.position + " Center " + map.areas[i].rect.center);
        }
    }


    //private void DisplayHLMap(HLMap map)
    //{
    //    Instantiate(map.areas[0].data.Island, new Vector3(map.areas[0].rect.center.x,0, map.areas[0].rect.center.y), Quaternion.identity, transform);
    //    //map.areas[0].Position = Vector3.zero;
    //    for (int i = 0; i < map.areas[0].Connections.Count; i++)
    //    {
    //        DisplayArea(map, 0, map.areas[0].Connections[i]);
    //    }
    //}

    //private void DisplayArea(HLMap map, int index, Connection connection)
    //{
    //    Instantiate(map.areas[connection.ToAreaIndex].data.Island, new Vector3( map.areas[connection.ToAreaIndex].rect.center.x,0, map.areas[connection.ToAreaIndex].rect.center.y), Quaternion.identity, transform);
    //    for (int i = 0; i < map.areas[connection.ToAreaIndex].Connections.Count; i++)
    //    {
    //        DisplayArea(map, connection.ToAreaIndex, map.areas[connection.ToAreaIndex].Connections[i]);
    //    }
    //}

}

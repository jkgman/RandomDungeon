using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HLGenerator
{

    /// <summary>
    /// Generate HLMap with given seed
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static HLMap GenerateHighLevelMap(SeedSettings seed)
    {
        //Setup Map and data
        HLMap dungeonMap = new HLMap(seed.Bridges);

        //Add begining and set rect
        dungeonMap.areas.Add(new HLArea(seed.RandomStart(),new Vector2(0,0)));

        //until seed count reached try adding rooms
        while (dungeonMap.areas.Count < seed.RoomCount-1)
        {
            AddRoom(dungeonMap, seed.RandomMiddle());
        }

        //Add Last Room
        AddRoom(dungeonMap, seed.RandomEnd());

        return dungeonMap;
    }

    /// <summary>
    /// Adds room to current map and hooks up connections
    /// </summary>
    /// <param name="roomData"></param>
    /// <param name="dungeonMap"></param>
    private static void AddRoom(HLMap dungeonMap, HLAreaData roomData) {

        HLArea addingArea = new HLArea(roomData, new Vector2(0, 0));
        //Find connection point
        Connection connection = FindValidConnection(dungeonMap, addingArea);
        //Add to map
        dungeonMap.areas.Add(addingArea);
        float rot = Vector2.SignedAngle(dungeonMap.areas[connection.ToAreaIndex].ConnectionPoints[connection.ToConnectionPoint].Rotation, dungeonMap.areas[connection.FromAreaIndex].ConnectionPoints[connection.FromConnectionPoint].Rotation) + 180;
        addingArea.RotateAroundRectPos(rot);
        addingArea.SetPosition(dungeonMap.areas[connection.ToAreaIndex].rect.center + dungeonMap.areas[connection.ToAreaIndex].ConnectionPoints[connection.ToConnectionPoint].Position -
            dungeonMap.areas[connection.FromAreaIndex].ConnectionPoints[connection.FromConnectionPoint].Position - dungeonMap.areas[connection.FromAreaIndex].rect.size / 2);
        int currentArea = dungeonMap.areas.Count - 1;

        //Set up connection on new area and connecting area
        dungeonMap.areas[currentArea].AddConnection(connection, true);
        dungeonMap.areas[connection.ToAreaIndex].AddConnection(Connection.ReverseConnection(connection), false);

    }

    /// <summary>
    /// Returns an area and the direction pointing to its valid connection in form of a connection
    /// </summary>
    /// <param name="dungeonMap"></param>
    /// <param name="area"></param>
    /// <returns></returns>
    private static Connection FindValidConnection(HLMap dungeonMap, HLArea newArea)
    {

        //make and scramble list of areas
        int[] areas = IntArrayExtensions.CreateArrayRange(0, dungeonMap.areas.Count);
        ArrayExtension.ShuffleArray(areas);

        //make and scramble list of roomData Connections
        int[] roomConnectionPoints = IntArrayExtensions.CreateArrayRange(0, newArea.ConnectionPoints.Count);
        ArrayExtension.ShuffleArray(roomConnectionPoints);

        //look over each area 
        for (int i = 0; i < areas.Length; i++)
        {

            HLArea currArea = dungeonMap.areas[areas[i]];

            //if has avaliable connection scramble list of open connections
            if (currArea.IsMaxConnectionsReached())
                continue;
            int[] currAreaConnectionPoints = IntArrayExtensions.CreateArrayRange(0, currArea.UnusedConnections.Count);
            ArrayExtension.ShuffleArray(currAreaConnectionPoints);
            //for all unused points check if any roomConnectionPoint is viable
            for (int j = 0; j < currAreaConnectionPoints.Length; j++)
            {
                for (int k = 0; k < roomConnectionPoints.Length; k++)
                {

                    //rotate new area to align conection directions
                    float degrees = Vector2.SignedAngle(currArea.UnusedConnections[currAreaConnectionPoints[j]].Rotation, newArea.ConnectionPoints[roomConnectionPoints[k]].Rotation);
                    Vector2 rectSize = Vector2Extensions.RotateDegree(newArea.rect.size, degrees);
                    Vector2 rectPos = currArea.rect.center + currArea.UnusedConnections[currAreaConnectionPoints[j]].Position;
                    rectPos += -Vector2Extensions.RotateDegree(newArea.ConnectionPoints[roomConnectionPoints[k]].Position, degrees);

                    if (dungeonMap.DoesntOverlap(new Rect(rectPos, rectSize)))
                    {
                        return new Connection(dungeonMap.areas.Count, roomConnectionPoints[k], areas[i], currArea.GetConnectionIndex( currArea.UnusedConnections[currAreaConnectionPoints[j]]));
                    }
                }
            }
        }
        throw new System.Exception("No connections found in HLGeneration");
    }

    
}

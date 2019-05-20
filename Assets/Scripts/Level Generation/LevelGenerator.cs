using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/// <summary>
/// Generate an inter connected map of rooms from the start of an arc to the end
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Range(0,360)]
    public float arcAngle = 45;
    public float arcWidth = 1;
    public float arcRadius = 1;
    public Room[] roomThresholds;
    public Room[] levelRooms;
    private float cullDistance = 0;
    public GameObject testObj;
    private List<Room> instantiatedRooms = new List<Room>();
    public GameObject tower;
    private void Start()
    {
        Generate(true);
        GameObject towerinst = Instantiate(tower,Vector3.zero, Quaternion.identity,transform);
        towerinst.transform.localScale = Vector3.one * arcRadius * 2;
        //if (arcAngle < 180)
        //{
        //    cullDistance = (new Vector3(Mathf.Sin(Mathf.Deg2Rad * (0 - arcAngle / 2)), 0, Mathf.Cos(Mathf.Deg2Rad * (0 - arcAngle / 2))) * arcRadius).z;
        //    cullDistance += (arcRadius - cullDistance) / 2;
        //}
        //instantiate tower
        //GenerateLevel();
        //connect rooms
        //place monsters
    }

    /// <summary>
    /// Calls the steps to generate the islands
    /// </summary>
    //void GenerateLevel() {
    //    PlaceRoom(roomThresholds, 0, 0);
    //    PlaceRoom(roomThresholds, arcAngle, 0);
    //    Room currentRoom = levelRooms[Random.Range(0, levelRooms.Length)];
    //    for (int x = 0; x < arcAngle; x++)
    //    {
    //        for (int y = 0; y < arcWidth; y++)
    //        {
    //            if (PlaceRoom(currentRoom, x, y))
    //            {
    //                currentRoom = levelRooms[Random.Range(0, levelRooms.Length)];
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Places a random Room from rooms at pos
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="pos"></param>
    bool PlaceRoom(Room[] rooms, Vector3 loc) {
        Room selectedRoom = rooms[Random.Range(0, rooms.Length)];
        if (PlaceRoom(selectedRoom, loc))
        {
            return true;
        }else
        {
            return false;
        }
        
    }
    bool PlaceRoom(Room room, Vector3 loc)
    {
        //for (int i = 0; i < instantiatedRooms.Count; i++)
        //{
        //    if ((instantiatedRooms[i].transform.position - pos).magnitude <= instantiatedRooms[i].RoomRadius + room.RoomRadius)
        //    {
        //        return false;
        //    }
        //}
        Room spawnRoom = Instantiate(room);
        instantiatedRooms.Add(spawnRoom);
        spawnRoom.transform.parent = transform;
        spawnRoom.transform.position = loc;
        spawnRoom.transform.forward = loc;
        return true;
    }

    
    public float roomSize = 10;
    public int rejectionSamples = 30;
    List<Vector2> points;
    Vector2 regionSize = Vector2.one;
    public void Generate(bool spawn) {
        //CleanUp();
        int startoffset = 2;
        Vector2[] startpoints = new Vector2[2];
        

        float yMax = arcRadius + arcWidth;
        float xMin;
        float xMax;
        float yMin;
        if (arcAngle > 180)
        {
            xMin = -arcRadius - arcWidth;
            xMax = -xMin;
            yMin = Mathf.Cos(Mathf.Deg2Rad * (arcAngle / 2)) * (arcRadius + arcWidth);
        }
        else {
            xMin= Mathf.Sin(Mathf.Deg2Rad * -(arcAngle / 2)) * (arcRadius + arcWidth);
            xMax = -xMin;
            yMin = Mathf.Cos(Mathf.Deg2Rad * (arcAngle / 2)) * arcRadius;
        }
        regionSize = new Vector3(xMax- xMin, yMax -yMin);
        Vector2 angleVec = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (startoffset - arcAngle / 2)), Mathf.Cos(Mathf.Deg2Rad * (startoffset - arcAngle / 2)));
        startpoints[0] = (angleVec * (arcRadius + 5)) + new Vector2(regionSize.x / 2, regionSize.y / 2- (arcRadius + arcWidth - regionSize.y / 2f));
        angleVec = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (arcAngle - startoffset - arcAngle / 2)), Mathf.Cos(Mathf.Deg2Rad * (arcAngle - startoffset - arcAngle / 2)));
        startpoints[1] = angleVec * (arcRadius +5) + new Vector2(regionSize.x / 2, regionSize.y / 2 - (arcRadius + arcWidth - regionSize.y / 2f)); ;
        points = PoissonDiscSampler.GeneratePoints(roomSize, regionSize, startpoints, rejectionSamples);
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 offset = new Vector3(0, 0, arcRadius + arcWidth - regionSize.y / 2f);
            Vector3 loc = new Vector3(points[i].x - regionSize.x / 2, 0, points[i].y - regionSize.y / 2) + offset;
            if (spawn && i < startpoints.Length)
            {
                PlaceRoom(roomThresholds, loc);
            }
            else if (spawn && loc.magnitude >= arcRadius && loc.magnitude <= arcRadius + arcWidth)
            {
                PlaceRoom(levelRooms, loc);
                //instantiatedRooms.Add(Instantiate(testObj, loc, Quaternion.identity, transform).GetComponent<Room>());
            }
        }
    }

    private void OnValidate()
    {
        Generate(false);
    }

    public float displayRadius = 1;
    public bool DrawOnlyInArc = false;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(0,0, arcRadius + arcWidth),displayRadius);
        Gizmos.DrawSphere(new Vector3(0, 0, -(arcRadius + arcWidth)), displayRadius);
        Gizmos.DrawSphere(new Vector3(-(arcRadius + arcWidth),0,0), displayRadius);
        Gizmos.DrawSphere(new Vector3(arcRadius + arcWidth, 0, 0), displayRadius);
        Gizmos.color = Color.grey;
        Vector3 offset = new Vector3(0, 0, arcRadius + arcWidth - regionSize.y / 2f);
        Gizmos.DrawWireCube(offset, new Vector3(regionSize.x, 0, regionSize.y));
        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Vector3 loc = new Vector3(point.x - regionSize.x / 2, 0, point.y - regionSize.y / 2);
                if (DrawOnlyInArc)
                {
                    loc += offset;
                    if (loc.magnitude >= arcRadius && loc.magnitude <= arcRadius+arcWidth)
                    {
                        Gizmos.DrawSphere(loc, displayRadius);
                    }
                }
                else
                {
                    Gizmos.DrawSphere(loc + offset, displayRadius);
                }
               
            }
        }
    }
}

[CustomEditor(typeof(LevelGenerator))]
public class customButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelGenerator myScript = (LevelGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            myScript.Generate(true);
        }
    }

}

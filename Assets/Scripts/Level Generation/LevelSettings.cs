using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class LevelSettings : ScriptableObject
{

    [Range(0, 360)]
    public float arcAngle = 45;
    public float arcWidth = 1;
    public float arcRadius = 1;
    public Room[] reqRooms;
    public Room[] rndRoomPool;
    public GameObject tower;
    
}

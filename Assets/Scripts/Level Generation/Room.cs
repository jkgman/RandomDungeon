using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public float RoomRadius;
    public Vector3[] connectionPoints;
    public Room[] ConnectedRooms;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, RoomRadius);
        for (int i = 0; i < connectionPoints.Length; i++)
        {
            Gizmos.DrawSphere(transform.position + connectionPoints[i], .5f);
        }
    }
}

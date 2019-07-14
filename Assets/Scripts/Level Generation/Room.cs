using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public float RoomRadius;
    public Vector3[] connectionPoints;
    public Room[] ConnectedRooms;
    public bool hardPos = false;
    public Node node;
    public List<Vector3> spawnpositions = new List<Vector3>();
    private void OnDrawGizmos()
    {
        for (int i = 0; i < spawnpositions.Count; i++)
        {
            Gizmos.DrawSphere(spawnpositions[i] + transform.position,.1f);
        }
    }
}

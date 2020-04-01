using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LL Single Room", menuName = "Generation/LL Areas/Single Room", order = 1)]
public class SingleRoomGenerator : LLGenerator
{

    [SerializeField]
    private SingleRoom room;
    [SerializeField]
    private Vector3 spawnPos;
    [SerializeField]
    private Vector3 spawnRot;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private bool spawnPlayer;
    private SingleRoom instantiatedRoom;
    protected override void StartGen()
    {
        instantiatedRoom = Instantiate(room, new Vector3(area.rect.center.x,0, area.rect.center.y), Quaternion.Euler(0, area.CurrentRotation, 0), area.AreaRoot.transform);
        if (spawnPlayer)
        {
            GameObject playerInstance = Instantiate(player);
            playerInstance.transform.position = new Vector3(area.rect.center.x, 0, area.rect.center.y) + spawnPos;
            playerInstance.transform.rotation =  Quaternion.Euler(spawnRot + new Vector3(0, area.CurrentRotation, 0));
        }
        instantiatedRoom.SetAllInactive();
        for (int i = 0; i < area.Connections.Count; i++)
        {
            instantiatedRoom.ActivateConnection(area.Connections[i].FromConnectionPoint);
        }
        for (int i = 0; i < area.BackConnection.Count; i++)
        {
            instantiatedRoom.ActivateConnection(area.BackConnection[i].FromConnectionPoint);
        }
        
    }
}

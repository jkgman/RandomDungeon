using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LL Single Room", menuName = "Generation/LL Areas/Single Room", order = 1)]
public class SingleRoomGenerator : LLGenerator
{

    [SerializeField]
    private SingleRoom room;
    private SingleRoom instantiatedRoom;
    protected override void StartGen()
    {
        instantiatedRoom = Instantiate(room, new Vector3(area.rect.center.x,0, area.rect.center.y), Quaternion.Euler(0, area.CurrentRotation, 0), area.AreaRoot.transform);
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

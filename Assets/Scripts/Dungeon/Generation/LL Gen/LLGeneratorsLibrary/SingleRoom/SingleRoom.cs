using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleRoom : MonoBehaviour
{
    [SerializeField]
    List<GameObject> Connections = new List<GameObject>();

    public void ActivateConnection(int index) {
        Connections[index].SetActive(true);
    }
    public void SetAllInactive() {
        for (int i = 0; i < Connections.Count; i++)
        {
            Connections[i].SetActive(false);
        }
    }
}

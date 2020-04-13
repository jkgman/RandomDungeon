using System.Collections.Generic;
using UnityEngine;

public class InstanceTracker : MonoBehaviour
{
    List<GameObject> instances = new List<GameObject>();

    public void RegisterInstances(GameObject obj) {

        instances.Add(obj);
    }

    public GameObject GetRandomInstance() {
        if (instances.Count >0)
        {
            return instances[Random.Range(0, instances.Count)];
        }
        else
        {
            throw new System.Exception("No instances to get in tracker " + name);
        }
    }
}

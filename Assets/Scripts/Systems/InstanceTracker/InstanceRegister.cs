using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceRegister : MonoBehaviour
{
    void Awake()
    {
        FindObjectOfType<InstanceTracker>().RegisterInstances(gameObject);
    }
}

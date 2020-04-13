using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    [SerializeField]
    GameObject dropingObject;
    [SerializeField]
    public bool drop;
    void OnDestroy()
    {
        if (drop)
        {
            Instantiate(dropingObject, transform.position, transform.rotation);
        }
    }
}

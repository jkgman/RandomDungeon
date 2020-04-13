using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    [SerializeField]
    GameObject dropingObject;
    [SerializeField]
    public bool drop;
    public void Drop()
    {
        if (drop && dropingObject)
        {
            Instantiate(dropingObject, transform.position, transform.rotation);
        }
    }
}

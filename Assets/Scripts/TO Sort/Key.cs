using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Key : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        KeyHolder holder = other.GetComponent<KeyHolder>();
        if (holder!= null)
        {
            holder.haskey = true;
            Destroy(gameObject);
        }
    }
}

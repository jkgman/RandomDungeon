using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRoom : MonoBehaviour
{
    [SerializeField]
    private GameObject playerprefab = default;
    [SerializeField]
    private Vector3 offset = Vector3.zero;
    [SerializeField]
    private GameObject cam = default;

    void Start()
    {
        GameObject player = Instantiate(playerprefab, transform.position + offset, Quaternion.identity);
        Instantiate(cam);
    }

}

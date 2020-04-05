using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField]
    private SpawnSystem spawnSystem;

    private void Awake()
    {
        spawnSystem.AddSpawn(this);
    }
}

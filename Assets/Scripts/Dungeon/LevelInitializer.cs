using Dungeon.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(InstanceTracker))]
public class LevelInitializer : MonoBehaviour
{
    [SerializeField]
    private SpawnSystem enemySpawns;
    [SerializeField]
    private SeedSettings seed;

    private InstanceTracker enenmyTracker;

    public Vector3 ExitLocation;
    public GameObject EnemyWithKey;

    void Awake()
    {
        Debug.Log("start");
        enenmyTracker = GetComponent<InstanceTracker>();

        //Initialize level layout
        HLMap hlMap = HLGenerator.GenerateHighLevelMap(seed);
        hlMap.GenerateLL();
        Debug.Log("generation finished");
        //Initialize navmesh
        //Spawn enemies
        enemySpawns.Spawn(seed.EnemyCount, seed.Enemy);
        Debug.Log("enemy spawn finished");
        //Add key to enemy
        EnemyWithKey = enenmyTracker.GetRandomInstance();
        EnemyWithKey.GetComponent<DropOnDestroy>().drop = true;
        Debug.Log("key set");
    }
    
}

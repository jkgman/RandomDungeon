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

    void Start()
    {
        enenmyTracker = GetComponent<InstanceTracker>();

        //Initialize level layout
        HLMap hlMap = HLGenerator.GenerateHighLevelMap(seed);
        hlMap.GenerateLL();
        //Initialize navmesh

        //Spawn enemies
        enemySpawns.Spawn(seed.EnemyCount, seed.Enemy);
        //Add key to enemy
        EnemyWithKey = enenmyTracker.GetRandomInstance();
        EnemyWithKey.GetComponent<DropOnDestroy>().drop = true;
    }
    
}

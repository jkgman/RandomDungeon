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

    private InstanceTracker enemyTracker;

    public Vector3 ExitLocation;
    public GameObject EnemyWithKey;

    void Awake()
    {
        enemyTracker = GetComponent<InstanceTracker>();

        //Initialize level layout
        HLMap hlMap = null;
        for (int i = 0; i < 10; i++)
        {
            try
            {
                hlMap = HLGenerator.GenerateHighLevelMap(seed);
                break;
            }
            catch (System.Exception)
            {
            }
        }
        hlMap.GenerateLL();
        Debug.Log("generation finished");



        //Initialize navmesh
        //Spawn enemies
        enemySpawns.Spawn(seed.EnemyCount, seed.Enemy);
        Debug.Log("enemy spawn finished");

        //Add key to enemy
        EnemyWithKey = enemyTracker.GetRandomInstance();
        EnemyWithKey.GetComponent<DropOnDestroy>().drop = true;
        Debug.Log("key set");
    }
    
}

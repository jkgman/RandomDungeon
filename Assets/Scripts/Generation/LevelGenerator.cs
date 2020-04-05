using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    SeedSettings seed;
    private HLMap _hlMap;
    [SerializeField]
    private SpawnSystem enemySpawns;
    private void Start()
    {
        Generate(seed);
        Populate();
    }

    private void Generate(SeedSettings seed)
    {
        _hlMap = HLGenerator.GenerateHighLevelMap(seed);
        _hlMap.GenerateLL();
    }
    private void Populate()
    {
        enemySpawns.Spawn(seed.EnemyCount, seed.Enemy);
    }

    private void PrintMapData(HLMap map) {
        for (int i = 0; i < map.areas.Count; i++)
        {
            Debug.Log("Size " + map.areas[i].rect.size + " Pos " + map.areas[i].rect.position + " Center " + map.areas[i].rect.center);
        }
    }

}

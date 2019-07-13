using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class LevelSettings : ScriptableObject
{

    [Range(0, 360)]
    public float arcAngle = 45;
    public float arcWidth = 1;
    public float arcRadius = 1;
    public Room[] reqRooms;
    public Room[] rndRoomPool;
    public GameObject tower;
    public List<EnemyList> enemies = new List<EnemyList>();
    [HideInInspector]
    public int EnemyCount;
    [HideInInspector]
    public bool forcedKey = false;
    [HideInInspector]
    public int requiredEnemy;

    void Start()
    {
        ScrapeEnemy();
    }
    void ScrapeEnemy()
    {
        bool forcedKey = false;
        int requiredEnemy;
        int total = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            total += enemies[i].CountForLevel;
            if (enemies[i].ForceKey)
            {
                forcedKey = true;
                requiredEnemy = i;
            }
        }
        EnemyCount = total;
    }
}
    [System.Serializable]
public class EnemyList {
    public GameObject EnemyPrefab;
    public int CountForLevel;
    public bool ForceKey;  
}

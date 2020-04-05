using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSystem", menuName = "Systems/SpawnSystem")]
public class SpawnSystem : ScriptableObject
{

    List<SpawnPoint> SpawnPositions = new List<SpawnPoint>();
    private void OnEnable()
    {
        SpawnPositions.Clear();
    }

    public void AddSpawn(SpawnPoint spawnPoint) {
        SpawnPositions.Add(spawnPoint);
    }

    public void Spawn(int count, GameObject spawnable) {
        if (count > SpawnPositions.Count)
        {
            Debug.LogWarning("More spawns requested than spawn points for object " + spawnable.name);
            count = SpawnPositions.Count;
        }
        ListExtensions.ShuffleList(SpawnPositions);
        for (int i = 0; i < count; i++)
        {
            Instantiate(spawnable, SpawnPositions[i].transform.position, SpawnPositions[i].transform.rotation);
        }
    }
}

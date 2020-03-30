using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HLMap
{
    public List<HLArea> areas = new List<HLArea>();
    public List<GameObject> areaBridges;

    public HLMap(List<GameObject> areaBridges) {
        this.areaBridges = areaBridges;
    }

    public GameObject GetRandomBridge() {
        return areaBridges[Random.Range(0, areaBridges.Count)];
    }

    public bool DoesntOverlap(Rect area) {
        for (int i = 0; i < areas.Count; i++)
        {
            if (areas[i].rect.Overlaps(area,true))
            {
                return false;
            }
        }
        return true;
    }
}

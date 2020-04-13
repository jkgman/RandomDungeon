using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HLMap
{
    public List<HLArea> areas = new List<HLArea>();

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

    public void GenerateLL() {
        for (int i = 0; i < areas.Count; i++)
        {
            areas[i].GenerateLL(i);
        }
    }
    
    public void PrintMapData(HLMap map) {
        for (int i = 0; i < map.areas.Count; i++)
        {
            Debug.Log("Size " + map.areas[i].rect.size + " Pos " + map.areas[i].rect.position + " Center " + map.areas[i].rect.center);
        }
    }
}

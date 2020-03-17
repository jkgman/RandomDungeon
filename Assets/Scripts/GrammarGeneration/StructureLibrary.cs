using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StructureLibrary")]
public class StructureLibrary : ScriptableObject
{

    [SerializeField]
    private List<Structure> structures = new List<Structure>();

    public Structure FindStructure(string name) {
        for (int i = 0; i < structures.Count; i++)
        {
            if (structures[i].name == name)
            {
                return structures[i];
            }
        }
        return null;
    }
}

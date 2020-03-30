using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HL Area", menuName = "Generation/HL Area Setting", order = 1)]
public class HLAreaSO : ScriptableObject
{
    [SerializeField]
    HLAreaData data;

    public HLAreaData Data {
        get {
            return data.GetCopy();
        }
    }
}

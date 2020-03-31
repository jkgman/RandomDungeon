using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Seed", menuName = "Generation/Seed Settings", order = 1)]
public class SeedSettings : ScriptableObject
{

    [SerializeField]
    int roomCount;
    [SerializeField]
    List<HLAreaSO> startAreaOptions = new List<HLAreaSO>();
    [SerializeField]
    List<HLAreaSO> middleAreaOptions = new List<HLAreaSO>();
    [SerializeField]
    List<HLAreaSO> endAreaOptions = new List<HLAreaSO>();
    public int RoomCount { get => roomCount; }
    public List<HLAreaSO> StartAreaOptions { get => startAreaOptions; }
    public List<HLAreaSO> MiddleAreaOptions { get => middleAreaOptions; }
    public List<HLAreaSO> EndAreaOptions { get => endAreaOptions; }

    public HLAreaData RandomStart() {
        if (StartAreaOptions.Count <= 0)
        {
            throw new System.Exception("No start areas detected in seed");
        }
        return StartAreaOptions[Random.Range(0, StartAreaOptions.Count)].Data;
    }
    public HLAreaData RandomMiddle()
    {
        if (MiddleAreaOptions.Count <= 0)
        {
            throw new System.Exception("No middle areas detected in seed");
        }
        return MiddleAreaOptions[Random.Range(0, MiddleAreaOptions.Count)].Data;
    }
    public HLAreaData RandomEnd()
    {
        if (EndAreaOptions.Count <= 0)
        {
            throw new System.Exception("No end areas detected in seed");
        }
        return EndAreaOptions[Random.Range(0, EndAreaOptions.Count)].Data;
    }
}

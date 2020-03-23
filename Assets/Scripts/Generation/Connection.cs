using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Connection
{
    public readonly int FromAreaIndex;
    public readonly int FromConnectionPoint;
    public readonly int ToAreaIndex;
    public readonly int ToConnectionPoint;
    public Connection(int fromAreaIndex, int fromConnectionPoint, int toAreaIndex, int toConnectionPoint) {
        FromAreaIndex = fromAreaIndex;
        FromConnectionPoint = fromConnectionPoint;
        ToAreaIndex = toAreaIndex;
        ToConnectionPoint = toConnectionPoint;
     }
}

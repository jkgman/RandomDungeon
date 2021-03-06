﻿using System.Collections;
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
    public void Print() {
        Debug.Log("Connection: From " + FromAreaIndex + " Point " + FromConnectionPoint + " To " + ToAreaIndex + " Point " + ToConnectionPoint);
    }
    /// <summary>
    /// Reverses a connection between two areas
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public static Connection ReverseConnection(Connection connection)
    {
        return new Connection(connection.ToAreaIndex, connection.ToConnectionPoint, connection.FromAreaIndex, connection.FromConnectionPoint);
    }
}

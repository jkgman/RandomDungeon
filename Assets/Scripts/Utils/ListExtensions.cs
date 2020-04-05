using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    /// <summary>
    /// Shuffles given array 
    /// </summary>
    /// <typeparam name="T">Type of array</typeparam>
    /// <param name="shuffleTarget">Target array to shuffle</param>
    public static void ShuffleList<T>(List<T> shuffleTarget)
    {
        int n = shuffleTarget.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = shuffleTarget[k];
            shuffleTarget[k] = shuffleTarget[n];
            shuffleTarget[n] = value;
        }
    }
}

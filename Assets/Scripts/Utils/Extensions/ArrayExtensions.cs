using UnityEngine;

public static class ArrayExtension
{
    /// <summary>
    /// Shuffles given array 
    /// </summary>
    /// <typeparam name="T">Type of array</typeparam>
    /// <param name="shuffleTarget">Target array to shuffle</param>
    public static void ShuffleArray<T>(T[] shuffleTarget)
    {
        int n = shuffleTarget.Length;
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
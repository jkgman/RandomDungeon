using System;
public static class IntArrayExtensions
{
    /// <summary>
    /// Creates an array that has values in order from start to end
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static int[] CreateArrayRange(int start, int count)
    {
        if (count < 0)
        {
            throw new Exception("Negative count given");
        }
        int[] arr = new int[count];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = i + start;
        }
        return arr;
    }/// <summary>
     /// Creates an array that has values in order from start to end
     /// </summary>
     /// <param name="start"></param>
     /// <param name="count"></param>
     /// <returns></returns>
    public static int[] CreateShuffledArrayRange(int start, int count)
    {
        if (count < 0)
        {
            throw new Exception("Negative count given");
        }
        int[] arr = new int[count];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = i + start;
        }
        ArrayExtension.ShuffleArray(arr);
        return arr;
    }
}
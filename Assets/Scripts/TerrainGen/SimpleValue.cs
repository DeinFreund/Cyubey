
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleValue : IGenerator
{
    protected readonly float value = 1;
    
    public SimpleValue(float value)
    {
        value = Math.Max(0, Math.Min(1, value));
        this.value = value;
    }
    public void fillArray(Coordinates coords, float[,,] array)
    {
        int x, y, z;
        for (x = 0; x < array.GetLength(0); x++)
        {
            for (y = 0; y < array.GetLength(1); y++)
            {
                for (z = 0; z < array.GetLength(2); z++)
                {
                    array[x, y, z] = value;
                }
            }
        }
    }
}

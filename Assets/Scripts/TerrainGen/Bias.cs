﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bias : IGenerator
{
    protected IGenerator child;
    protected readonly float power = 1;

    //bias [0,1]: 
    //0.5->0: all values are biased to 0
    //0.5->1: all values are biased to 1
    public Bias(IGenerator child, float bias)
    {
        this.child = child;
        bias = Math.Max(0, Math.Min(1, bias));
        this.power = -Mathf.Log(bias) / Mathf.Log(2);
    }
    public void fillArray(Coordinates coords, float[,,] array)
    {
        child.fillArray(coords, array);
        int x, y, z;
        for (x = 0; x < array.GetLength(0); x++)
        {
            for (y = 0; y < array.GetLength(1); y++)
            {
                for (z = 0; z < array.GetLength(2); z++)
                {
                    array[x, y, z] = Mathf.Pow(array[x, y, z], power);
                }
            }
        }
    }
}

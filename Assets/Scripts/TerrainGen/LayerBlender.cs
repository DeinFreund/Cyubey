
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerBlender : IGenerator
{
    protected const float MERGE_DIST = 10;
    protected SortedList<int, Layer> layers = new SortedList<int, Layer>();

    public LayerBlender(params Layer[] children)
    {
        foreach (Layer l in children)
        {
            addLayer(l);
        }
    }

    public void addLayer(Layer l)
    {
        layers.Add(l.startingHeight, l);
    }


    private Layer previous, next;
    private int min, max, mid;
    private float weight;
    public float getValue(Coordinates coords)
    {
        max = layers.Count;
        min = 0;
        while (max - min > 1)
        {
            mid = (max + min) >> 1;
            if (layers.Keys[mid] > coords.y)
            {
                max = mid;
            }else
            {
                min = mid;
            }
        }
        previous = layers.Values[min];
        if (max >= layers.Count)
        {
            weight = 1;
        }
        else
        {
            next = layers.Values[max];
            weight = Math.Min(1, (next.startingHeight - coords.y) / MERGE_DIST);
        }
        if (weight > 0.99f) return previous.terrainGenerator.getValue(coords);
        return weight * previous.terrainGenerator.getValue(coords) + (1 - weight) * next.terrainGenerator.getValue(coords);
    }
}

public class Layer
{
    public readonly int startingHeight;
    public readonly IGenerator terrainGenerator;

    public Layer(IGenerator terrainGenerator, int startingHeight)
    {
        this.startingHeight = startingHeight;
        this.terrainGenerator = terrainGenerator;
    }
}
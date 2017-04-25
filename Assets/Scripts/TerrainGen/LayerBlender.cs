
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerBlender : IGenerator
{
    protected const float MERGE_DIST = 10;
    protected List< Layer> layers = new List<Layer>();

    public LayerBlender(params Layer[] children)
    {
        foreach (Layer l in children)
        {
            addLayer(l, false);
        }
    }

    protected void sort()
    {
        layers.Sort((a,b) => a.startingHeight.CompareTo(b.startingHeight));
    }

    protected void addLayer(Layer l, bool sort)
    {
        layers.Add(l);
        if (sort) this.sort();
    }

    public void addLayer(Layer l)
    {
        addLayer(l, true);
    }


    public float getValue(Coordinates coords)
    {
        Layer previous, next = null;
        int min, max, mid;
        float weight;
        max = layers.Count;
        min = 0;
        while (max - min > 1)
        {
            mid = (max + min) >> 1;
            if (layers[mid].startingHeight > coords.y)
            {
                max = mid;
            }
            else
            {
                min = mid;
            }
        }
        previous = layers[min];
        if (max >= layers.Count)
        {
            weight = 1;
        }
        else
        {
            next = layers[max];
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
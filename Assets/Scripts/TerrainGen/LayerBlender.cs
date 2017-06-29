
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LayerBlender : IGenerator
{

    private static HashSet<float[,,]> floatArrays = new HashSet<float[,,]>();

    private static float[,,] getFloat()
    {
        lock (floatArrays)
        {
            if (floatArrays.Count == 0) return new float[TerrainChunk.size * Chunk.size, 1, TerrainChunk.size * Chunk.size];
            float[,,] ret = floatArrays.First();
            floatArrays.Remove(ret);
            return ret;
        }
    }
    private static void disposeFloat(float[,,] element)
    {
        lock (floatArrays)
        {
            floatArrays.Add(element);
        }
    }

    protected const float MERGE_DIST = 20;
    protected List<Layer> layers = new List<Layer>();

    public LayerBlender(params Layer[] children)
    {
        foreach (Layer l in children)
        {
            addLayer(l, false);
        }
    }

    protected void sort()
    {
        layers.Sort((a, b) => a.startingHeight.CompareTo(b.startingHeight));
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


    public void fillArray(Coordinates coords, float[,,] array)
    {
        TimeSpan total = new TimeSpan();
        DateTime start2 = DateTime.Now;
        int x, y, z;
        for (y = 0; y < array.GetLength(1); y++)
        {
            Layer previous, next = null;
            int min, max, mid;
            float weight;
            max = layers.Count;
            min = 0;
            while (max - min > 1)
            {
                mid = (max + min) >> 1;
                if (layers[mid].startingHeight > coords.y + y)
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
                weight = Math.Min(1, (next.startingHeight - coords.y - y) / MERGE_DIST);
            }
            if (weight > 0.99f) {
                float[,,] layer = getFloat();
                DateTime start = DateTime.Now;
                previous.terrainGenerator.fillArray(new Coordinates(coords.x, coords.y + y, coords.z), layer);

                total += (DateTime.Now.Subtract(start));

                for (x = 0; x < array.GetLength(0); x++)
                {
                    for (z = 0; z < array.GetLength(2); z++)
                    {
                        array[x, y, z] = layer[x,0,z];
                    }
                }
                disposeFloat(layer);
            }
            else
            {
                float[,,] previousLayer = getFloat();
                float[,,] nextLayer = getFloat();

                DateTime start = DateTime.Now;
                previous.terrainGenerator.fillArray(new Coordinates(coords.x, coords.y + y, coords.z), previousLayer);
                next.terrainGenerator.fillArray(new Coordinates(coords.x, coords.y + y, coords.z), nextLayer);

                total += (DateTime.Now.Subtract(start));

                for (x = 0; x < array.GetLength(0); x++)
                {
                    for (z = 0; z < array.GetLength(2); z++)
                    {
                        array[x, y, z] = weight * previousLayer[x, 0, z] + (1 - weight) * nextLayer[x, 0, z];
                    }
                }
                disposeFloat(previousLayer);
                disposeFloat(nextLayer);
            }
        }

        Debug.Log("Perlin took " + total + " of " + DateTime.Now.Subtract(start2) + " to fill " + array.GetLength(0) + " | " + array.GetLength(1) + " | " + array.GetLength(2));
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
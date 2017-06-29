using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class TerrainChunk : IDisposable
{
    public const int size = 12; //sidelength of cube, measured in chunks

    private static HashSet<short[,,]> shortArrays = new HashSet<short[,,]>();
    private static HashSet<float[,,]> floatArrays = new HashSet<float[,,]>();

    
    private static readonly LocationFreer terrain = new LocationFreer(
        new LayerBlender(
            new Layer(new Bias(new Perlin(42, new float[] {  2f }, 4), 0.58f), -1),
            new Layer(new Perlin(42, new float[] {0.2f, 0.5f }, 4), 0)
        ));//*/

    //private static readonly LocationFreer terrain = new LocationFreer(new SimpleValue(1));
    static TerrainChunk()
    {
        terrain.freeLocation(new Coordinates(0, 0, 0), 4, 12);
    }

    private readonly Coordinates pos;

    private static short[,,] getShort()
    {
        lock (shortArrays)
        {
            if (shortArrays.Count == 0) return new short[size * Chunk.size, size * Chunk.size, size * Chunk.size];
            short[,,] ret = shortArrays.First();
            shortArrays.Remove(ret);
            return ret;
        }
    }
    private static void disposeShort(short[,,] element)
    {
        lock (shortArrays)
        {
            shortArrays.Add(element);
        }
    }

    private static float[,,] getFloat()
    {
        lock (floatArrays)
        {
            if (floatArrays.Count == 0) return new float[size * Chunk.size, size * Chunk.size, size * Chunk.size];
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

    private short[,,] blocks = getShort();
    private bool generated = false;


    public TerrainChunk(Coordinates tcpos)
    {
        this.pos = tcpos;
        Thread worker = new Thread(generate);
        worker.Start();
    }

    public bool isGenerated()
    {
        return generated;
    }

    public short getBlock(Coordinates coords)
    {
        if (!generated) throw new Exception("Queried non generated terrain");
        return blocks[coords.x, coords.y, coords.z];
    }

    private void generate()
    {
        Debug.Log("Generating " + pos);
        DateTime start2 = DateTime.Now;
        float[,,] ground = getFloat();
        terrain.fillArray(pos * (size * Chunk.size), ground);

        for (int x = 0; x < size * Chunk.size; x++)
        {
            for (int y = 0; y < size * Chunk.size; y++)
            {
                for (int z = 0; z < size * Chunk.size; z++)
                {
                    blocks[x, y, z] = ground[x, y, z] > 0.42 ? Rock.ID : Air.ID;
                }
            }
        }
        for (int x = 0; x < size * Chunk.size; x++)
        {
            for (int y = 1; y < size * Chunk.size; y++)
            {
                for (int z = 0; z < size * Chunk.size; z++)
                {
                    //if (blocks[x, y, z] == Air.ID && blocks[x, y - 1, z] == Rock.ID && y < 3) blocks[x, y, z] = Water.ID;
                }
            }
        }
        generated = true;
        TerrainCompositor.TerrainGenerated(pos);
        disposeFloat(ground);
        Debug.Log("Genearting took " + DateTime.Now.Subtract(start2) + " to fill " + ground.GetLength(0) + " | " + ground.GetLength(1) + " | " + ground.GetLength(2));
    }

    public void Dispose()
    {
        disposeShort(blocks);
    }
}


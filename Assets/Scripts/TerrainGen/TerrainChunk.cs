using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public class TerrainChunk : IDisposable
{
    public const int size = 16; //sidelength of cube, measured in chunks

    private static HashSet<short[,,]> shortArrays = new HashSet<short[,,]>();
    private static HashSet<float[,,]> floatArrays = new HashSet<float[,,]>();


    private static readonly LocationFreer terrain = new LocationFreer(
        new LayerBlender(
            new Layer(new Bias(new Perlin(42, new float[] { 0.5f, 2f }, 4), 0.58f), -1),
            new Layer(new Perlin(42, new float[] { 0.05f, 0.1f, 0.2f, 0.5f }, 4), 0)
        ));

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

    public short getBlock(Coordinates coords)
    {
        return generated ? blocks[coords.x, coords.y, coords.z] : short.MinValue;
    }

    private void generate()
    {
        float[,,] terrain = getFloat();

        for (int x = 0; x < size * Chunk.size; x++)
        {
            for (int y = 0; y < size * Chunk.size; y++)
            {
                for (int z = 0; z < size * Chunk.size; z++)
                {
                    blocks[x, y, z] = terrain[x, y, z] < 0.42 ? Block.ID : Air.ID;
                }
            }
        }
        generated = true;
        TerrainCompositor.TerrainGenerated(pos);
    }

    public void Dispose()
    {
        disposeShort(blocks);
    }
}


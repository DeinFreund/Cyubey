using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TerrainCompositor
{
    private static Dictionary<Coordinates, TerrainChunk> loadedTerrainChunks = new Dictionary<Coordinates, TerrainChunk>();
    private static Dictionary<Coordinates, HashSet<Chunk>> correspondingChunks = new Dictionary<Coordinates, HashSet<Chunk>>();

    public static short GetBlock(Coordinates coords)
    {
        Coordinates tcpos = coords.div_floor(TerrainChunk.size * Chunk.size);
        if (!loadedTerrainChunks.ContainsKey(tcpos)) throw new Exception("Queried non generated terrain chunk");
        return loadedTerrainChunks[tcpos].getBlock(coords - tcpos * (TerrainChunk.size * Chunk.size));
    }

    public static bool GetBlockReady(Coordinates coords)
    {
        Coordinates tcpos = coords.div_floor(TerrainChunk.size * Chunk.size);
        if (!loadedTerrainChunks.ContainsKey(tcpos)) return false;
        return loadedTerrainChunks[tcpos].isGenerated();
    }

    public static void ChunkLoaded(Chunk chunk)
    {
        Coordinates tcpos = chunk.getCoordinates().div_floor(TerrainChunk.size);
        if (!loadedTerrainChunks.ContainsKey(tcpos))
        {
            loadedTerrainChunks[tcpos] = new TerrainChunk(tcpos);
            correspondingChunks[tcpos] = new HashSet<Chunk>();
        }
        correspondingChunks[tcpos].Add(chunk);
    }

    public static void ChunkUnloaded(Chunk chunk)
    {
        Coordinates tcpos = chunk.getCoordinates().div_floor(TerrainChunk.size);
        correspondingChunks[tcpos].Remove(chunk);
        if (correspondingChunks[tcpos].Count == 0)
        {
            loadedTerrainChunks[tcpos].Dispose();
            loadedTerrainChunks.Remove(tcpos);
        }
    }

    public static void TerrainGenerated(Coordinates tcpos)
    {
        if (!correspondingChunks.ContainsKey(tcpos))
        {
            Debug.LogWarning("TChunk unloaded during generation at pos " + tcpos);
            return;
        }
        foreach (Chunk c in correspondingChunks[tcpos])
        {
            c.TerrainReady();
        }
    }
}


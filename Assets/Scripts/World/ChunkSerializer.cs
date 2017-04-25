﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

class ChunkSerializer
{
    public static byte[] serializeBlocks(BitArray[,] blocksModified, Block[,,] blocks)
    {

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memStr = new MemoryStream();
        bf.Serialize(memStr, blocksModified);
        for (int sx = 0; sx < Chunk.size; sx++)
            for (int sy = 0; sy < Chunk.size; sy++)
                if (blocksModified[sx, sy] != null)
                    for (int sz = 0; sz < Chunk.size; sz++)
                        if (blocksModified[sx, sy][sz])
                            bf.Serialize(memStr, blocks[sx,sy,sz]);
        memStr.Position = 0;
        return memStr.ToArray();
    }

    public static BitArray[,] deserializeBlocks(Block[,,] blocks, byte[] serialized, int length)
    {
        if (length == 0) return null;
        MemoryStream memStr = new MemoryStream(serialized, 0, length);
        memStr.Position = 0;
        BinaryFormatter bf = new BinaryFormatter();
        BitArray[,] blocksModified = (BitArray[,])bf.Deserialize(memStr);
        if (blocksModified.GetLength(0) != Chunk.size || blocksModified.GetLength(1) != Chunk.size || blocksModified.GetLength(2) != Chunk.size)
        {
            Debug.LogError("Corrupt save, resetting chunk");
            return null;
        }
        for (int sx = 0; sx < Chunk.size; sx++)
            for (int sy = 0; sy < Chunk.size; sy++)
                if (blocksModified[sx, sy] != null)
                    for (int sz = 0; sz < Chunk.size; sz++)
                        if (blocksModified[sx, sy][sz])
                            blocks[sx, sy, sz] = (Block)bf.Deserialize(memStr);
        return blocksModified;
    }

    public static byte[] serializeBlock(Block block)
    {

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memStr = new MemoryStream();
        bf.Serialize(memStr, block);
        memStr.Position = 0;
        return memStr.ToArray();
    }

    public static Block deserializeBlock(byte[] serialized)
    {
        if (serialized.Length == 0)
        {
            Debug.LogError("Null block given");
            return new Block();
        }
        MemoryStream memStr = new MemoryStream(serialized);
        memStr.Position = 0;
        BinaryFormatter bf = new BinaryFormatter();
        return (Block)bf.Deserialize(memStr);
    }

    private static SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

    public static byte[] hash(byte[] serialized)
    {
        return sha1.ComputeHash(serialized);
    }
}
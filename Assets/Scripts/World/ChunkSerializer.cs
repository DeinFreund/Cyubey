using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

class ChunkSerializer
{
    private static BitArray empty = new BitArray(Chunk.size);
    private static byte[] nothing = new byte[0];

    public static byte[] serializeBlocks(BitArray[,] blocksModified, Block[,,] blocks)
    {
        if (blocksModified == null) return nothing;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream memStr = new MemoryStream()) {
            using (DeflateStream compressed = new DeflateStream(memStr, CompressionMode.Compress))
            {
                bf.Serialize(compressed, blocksModified);
                for (int sx = 0; sx < Chunk.size; sx++)
                    for (int sy = 0; sy < Chunk.size; sy++)
                        if (blocksModified[sx, sy] != null)
                            for (int sz = 0; sz < Chunk.size; sz++)
                                if (blocksModified[sx, sy][sz])
                                    blocks[sx, sy, sz].serialize(memStr, bf);
            }
            return memStr.ToArray();
        }
    }

    public static BitArray[,] deserializeBlocks(Block[,,] blocks, byte[] serialized, int length, bool serverBlocks)
    {
        if (length == 0) return null;
        try
        {
            using (MemoryStream memStr = new MemoryStream(serialized, 0, length))
            {
                using (DeflateStream compressed = new DeflateStream(memStr, CompressionMode.Decompress))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    BitArray[,] blocksModified = (BitArray[,])bf.Deserialize(compressed);
                    if (blocksModified.GetLength(0) != Chunk.size || blocksModified.GetLength(1) != Chunk.size)
                    {
                        Debug.LogWarning("Corrupt save, resetting chunk");
                        return null;
                    }
                    //Debug.Log("Deserialized chunk");
                    for (int sx = 0; sx < Chunk.size; sx++)
                        for (int sy = 0; sy < Chunk.size; sy++)
                            if (blocksModified[sx, sy] != null)
                                for (int sz = 0; sz < Chunk.size; sz++)
                                    if (blocksModified[sx, sy][sz])
                                        blocks[sx, sy, sz] = deserializeBlock(compressed, bf, serverBlocks);
                    return blocksModified;
                }
            }
        }catch(Exception ex)
        {
            Debug.LogWarning("Corrupt save, resetting chunk: " + ex);
            return null;
        }
    }

    public static byte[] serializeBlock(Block block)
    {

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memStr = new MemoryStream();
        block.serialize(memStr, bf);
        memStr.Position = 0;
        return memStr.ToArray();
    }

    public static Block deserializeBlock(byte[] serialized, bool serverBlock)
    {
        if (serialized.Length == 0)
        {
            Debug.LogError("Null block given");
            return new Rock(new Coordinates(), serverBlock);
        }
        MemoryStream memStr = new MemoryStream(serialized);
        memStr.Position = 0;
        BinaryFormatter bf = new BinaryFormatter();
        return deserializeBlock(memStr, bf, serverBlock);
    }

    private static Block deserializeBlock(Stream memStr, BinaryFormatter bf, bool serverBlock)
    {
        return (bf.Deserialize(memStr) as BlockData).reconstruct(serverBlock);
    }

    private static SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

    public static byte[] hash(byte[] serialized)
    {
        return sha1.ComputeHash(serialized);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class ChunkData : UDPNetworkMessage
{
    public const byte ID = 2;
    public readonly byte id = ID;

    public override byte getMessageID()
    {
        return id;
    }

    public byte[] chunkData;
    public int x, y, z;

    public ChunkData(int affectedPlayer, int x, int y, int z, byte[] chunkData) : base(affectedPlayer)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.chunkData = chunkData;
    }
}
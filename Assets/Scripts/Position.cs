using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct Position
{
    public readonly int x, y, z;
    private Chunk chunk;
    int xOff, yOff, zOff;
    
    public Position(int locX, int locY, int locZ, Chunk chunk)
    {
        this.chunk = chunk;
        xOff = chunk.getXOffset();
        yOff = chunk.getYOffset();
        zOff = chunk.getZOffset();
        this.x = locX + xOff;
        this.y = locY + yOff;
        this.z = locZ + zOff;
    }
    public Position(int absX, int absY, int absZ)
    {
        this.chunk = World.getChunk(new Coordinates((div_floor(absX, Chunk.size)), div_floor(absY, Chunk.size), div_floor(absZ, Chunk.size)));
        xOff = (div_floor(absX, Chunk.size)) * Chunk.size;
        yOff = (div_floor(absY, Chunk.size)) * Chunk.size;
        zOff = (div_floor(absZ, Chunk.size)) * Chunk.size;
        this.x = absX;
        this.y = absY;
        this.z = absZ;
    }

    public int getX()
    {
        return x;
    }
    public int getY()
    {
        return y;
    }
    public int getZ()
    {
        return z;
    }

    public int getRelativeX()
    {
        return x - xOff;
    }
    public int getRelativeY()
    {
        return y - yOff;
    }
    public int getRelativeZ()
    {
        return z - zOff;
    }
    public Coordinates getRelativeCoords()
    {
        return new Coordinates(getRelativeX(), getRelativeY(), getRelativeZ());
    }
    public Chunk getChunk()
    {
        return chunk;
    }
    public Coordinates getChunkCoordinates()
    {
        return new Coordinates((div_floor(x, Chunk.size)), div_floor(y, Chunk.size), div_floor(z, Chunk.size));
    }
    public Position offset(Coordinates offset)
    {
        return this.offset(offset.getX(), offset.getY(), offset.getZ());
    }
    public Position offset(int x, int y, int z)
    {
        return new Position(getX() + x, getY() + y, getZ() + z);
    }
    public Position above()
    {
        return offset(0, 1, 0);
    }
    public Position below()
    {
        return offset(0, -1, 0);
    }
    public Block getBlock()
    {
        if (chunk == null) return NoBlock.noblock;
        return chunk.getBlock(getRelativeX(), getRelativeY(), getRelativeZ());
    }
    public static Block getBlockAt(int x, int y, int z)
    {
        Coordinates cc = new Coordinates((div_floor(x, Chunk.size)), div_floor(y, Chunk.size), div_floor(z, Chunk.size));
        Chunk c = World.getChunk(cc);
        return c == null ? NoBlock.noblock : c.getBlock((x % Chunk.size + Chunk.size) % Chunk.size, (y % Chunk.size + Chunk.size) % Chunk.size, (z % Chunk.size + Chunk.size) % Chunk.size);
    }

    public static int div_floor(int x, int y)
    {
        int q = x / y;
        int r = x % y;
        if ((r != 0) && ((r < 0) != (y < 0))) --q;
        return q;
    }

    public static implicit operator Coordinates(Position coords)
    {
        return new Coordinates(coords.getX(), coords.getY(), coords.getZ());
    }

    public static implicit operator Vector3(Position coords)
    {
        return new Vector3(coords.getX(), coords.getY(), coords.getZ());
    }

}


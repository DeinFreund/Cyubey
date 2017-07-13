using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public abstract class BlockData
{
    public Coordinates coords;

    public abstract Block reconstruct(bool serverBlock);
}

public abstract class Block
{
    protected readonly BlockData data;
    protected Coordinates coords { get { return data.coords; } set { data.coords = value; } }
    public readonly bool isServerBlock;
    //protected Position pos;
    //protected GameObject block;

    public const short ID = 0;
    //public static Type type = typeof(Block);


    public Block(Coordinates coords, bool serverBlock, BlockData data)
    {
        this.data = data;
        this.data.coords = coords;
        this.isServerBlock = serverBlock;
    }


    public abstract Field unload();

    public void serialize(Stream memStr, BinaryFormatter bf)
    {
        bf.Serialize(memStr, data);
    }

    public void setDebugColor(Color color)
    {
        /*if (!Profiler.enabled || block == null) return;
        block.GetComponent<MeshRenderer>().material.color = color;*/
    }

    public Coordinates getCoordinates()
    {
        return coords;
    }

    public Position getPosition()
    {
        return (Position)coords;
    }


    public GameObject getGameObject()
    {
        return null;
    }

    public virtual short getID()
    {
        return ID;
    }

    public short getMeshID()
    {
        return getMeshID(coords);
    }

    public virtual bool isTransparent()
    {
        return true;
    }

    public abstract void applyAction(BlockAction action);

    public abstract short getMeshID(Coordinates coords);

    public override string ToString()
    {
        return base.ToString() + "@" + coords + " #" + GetHashCode();
    }
}

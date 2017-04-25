using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Block
{

    protected Coordinates coords;
    //protected Position pos;
    //protected GameObject block;

    public static short ID = 0;
    //public static Type type = typeof(Block);


    public Block()
    {
    }

    public virtual void init(Coordinates coords)
    {
        this.coords = coords;
    }

    public virtual Field unload()
    {
        return null;
    }


    public void setDebugColor(Color color)
    {
        /*if (!Profiler.enabled || block == null) return;
        block.GetComponent<MeshRenderer>().material.color = color;*/
    }

    protected virtual void instantiate()
    {
        //if (block == null) block = BlockFactory.instantiate(this);
    }

    protected virtual void destroy()
    {
        /*if (block != null)
        {
            BlockFactory.destroy(this);
            block = null;
        }*/
    }

    public Coordinates getCoordinates()
    {
        return coords;
    }

    public Position getPosition()
    {
        return new Position(coords.x, coords.y, coords.z);
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

    public virtual short getMeshID(Coordinates coords)
    {
        return (short)((coords.GetHashCode() % 13 + 13) % 13 % 4);
    }

    public virtual bool isTransparent()
    {
        return false;
    }

}

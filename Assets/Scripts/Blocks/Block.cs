using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Block
{

    protected Coordinates coords;
    //protected Position pos;
    //protected GameObject block;

    public static int ID = 0;
    public static Type type = typeof(Block);


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

    public virtual int getID()
    {
        return ID;
    }

    public int getMeshID()
    {
        return getMeshID(coords);
    }

    public virtual int getMeshID(Coordinates coords)
    {
        return (coords.GetHashCode() % 13 + 13) % 13 % 3;
    }

    public virtual bool isTransparent()
    {
        return false;
    }
    
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public abstract class Block
{

    protected Coordinates coords;
    //protected Position pos;
    //protected GameObject block;

    public const short ID = 0;
    //public static Type type = typeof(Block);


    public Block(Coordinates coords)
    {
        this.coords = coords;
    }


    public abstract Field unload();


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

    public virtual bool isTransparent()
    {
        return true;
    }

    public abstract short getMeshID(Coordinates coords);
    

}

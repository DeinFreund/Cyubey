using UnityEngine;
using System.Collections;
using System;

[Serializable]
public abstract class Liquid : Block
{
    
    public new const short ID = 0;

    private int meshID = 4;

    public Liquid(Coordinates coords) : base(coords)
    {

    }

    public override short getMeshID(Coordinates coords)
    {
        return 4;
    }

    public abstract Transform getPrefab();

    public override short getID()
    {
        return ID;
    }
}

using UnityEngine;
using System.Collections;
using System;

[Serializable]
public abstract class Solid : Block
{
    
    public new const short ID = 0;

    public Solid(Coordinates coords) : base(coords)
    {

    }

    public override short getID()
    {
        return ID;
    }

    public override bool isTransparent()
    {
        return false;
    }
}

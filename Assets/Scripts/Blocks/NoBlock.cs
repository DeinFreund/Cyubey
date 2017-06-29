using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class NoBlock : Block
{

    public static Block noblock = new NoBlock(new Coordinates());
    public new const short ID = -1;
    

    public NoBlock(Coordinates coords) : base(coords)
    {

    }

    public override short getMeshID(Coordinates coords)
    {
        return ID;
    }

    public override short getID()
    {
        return ID;
    }

    public override Field unload()
    {
        return null;
    }
}

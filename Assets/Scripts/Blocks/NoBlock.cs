using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class NoBlock : Block
{

    public static Block noblock = new NoBlock(new Coordinates());
    public new const short ID = -1;

    protected override void instantiate()
    {

    }

    public NoBlock(Coordinates coords) : base(coords)
    {

    }

    public override short getMeshID(Coordinates coords)
    {
        return ID;
    }

    public override bool isTransparent()
    {
        return true;
    }

    public override short getID()
    {
        return ID;
    }
}

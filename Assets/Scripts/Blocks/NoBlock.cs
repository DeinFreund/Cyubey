using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class NoBlock : Block
{

    public static Block noblock = new NoBlock();
    public static new short ID = -1;

    protected override void instantiate()
    {

    }

    public NoBlock() : base()
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

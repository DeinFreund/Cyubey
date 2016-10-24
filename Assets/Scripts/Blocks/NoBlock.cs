using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoBlock : Block
{

    public static Block noblock = new NoBlock();
    public static new int ID = -1;

    protected override void instantiate()
    {

    }

    public NoBlock() : base()
    {

    }

    public override int getMeshID(Coordinates coords)
    {
        return ID;
    }

    public override bool isTransparent()
    {
        return true;
    }

    public override int getID()
    {
        return ID;
    }
}

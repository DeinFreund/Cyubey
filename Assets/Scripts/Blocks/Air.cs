using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Air : Block {


    public static new int ID = -1;
    public static new Type type = typeof(Air);

    protected override void instantiate()
    {

    }

    public Air() : base()
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class Air : Block {


    public static new short ID = -1;
    //public static new Type type = typeof(Air);

    protected override void instantiate()
    {

    }

    public Air() : base()
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

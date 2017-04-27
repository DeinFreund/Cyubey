using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class Air : Block {


    public new const short ID = -1;
    //public static new Type type = typeof(Air);

    protected override void instantiate()
    {

    }

    public Air(Coordinates coords) : base(coords)
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

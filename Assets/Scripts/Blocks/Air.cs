using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class Air : Block {


    public new const short ID = -1;

    public Air(Coordinates coords) : base(coords)
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

using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Stone : Block
{

    public static new short ID = 1;

    public Stone() : base()
    {

    }

    public override short getMeshID(Coordinates coords)
    {
        return 1;
    }

    public override short getID()
    {
        return ID;
    }
}

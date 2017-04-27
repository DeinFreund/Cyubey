using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Stone : Block
{
    
    public new const short ID = 1;

    public Stone(Coordinates coords) : base(coords)
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

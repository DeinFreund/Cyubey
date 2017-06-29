using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Rock : Solid
{
    
    public new const short ID = 1;


    public Rock(Coordinates coords) : base(coords)
    {
    }
    

    public override Field unload()
    {
        return null;
    }
    
    public override short getID()
    {
        return ID;
    }

    public override short getMeshID(Coordinates coords)
    {
        return (short)((coords.GetHashCode() % 13 + 13) % 13 % 4);
    }

}

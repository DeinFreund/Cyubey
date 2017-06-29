using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Water : Liquid
{
    
    public new const short ID = 2;

    public Water(Coordinates coords) : base(coords)
    {

    }


    public override short getID()
    {
        return ID;
    }

    public override Transform getPrefab()
    {
        return BlockFactory.waterPrefab;
    }

    public override Field unload()
    {
        return null;
    }
}

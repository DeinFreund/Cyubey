using UnityEngine;
using System.Collections;
using System;

[Serializable]
public abstract class SolidData : BlockData
{
}


[Serializable]
public abstract class Solid : Block
{
    
    public new const short ID = 0;

    public Solid(Coordinates coords, bool serverBlock, SolidData data) : base(coords, serverBlock, data)
    {

    }

    public override short getID()
    {
        return ID;
    }

    public override bool isTransparent()
    {
        return false;
    }
    
    public override void applyAction(BlockAction action)
    {
        UpdatePressure pu = action as UpdatePressure;
        if (pu != null)
        {
            Liquid liq = pu.changed as Liquid;
            if (liq != null)
            {
                float otherPressure = liq.getPressure();
                if (liq.getCoordinates().y > coords.y)
                {
                    otherPressure += liq.density;
                }

            }
        }
        else
        {
            Debug.LogError("Unknown action: " + action);
        }
    }
}

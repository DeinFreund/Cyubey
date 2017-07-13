using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class WaterData : LiquidData
{
    public override Block reconstruct(bool serverBlock)
    {
        return new Water(coords, serverBlock, pressure);
    }
}

public class Water : Liquid
{

    protected new WaterData data { get { return base.data as WaterData; } }

    public new const short ID = 2;
    public readonly new float density = 1;

    public Water(Coordinates coords, bool serverBlock, float initialPressure) : base(coords, serverBlock, new WaterData(), initialPressure)
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

    protected override Liquid spreadTo(Position to, float initialLevel)
    {
        Debug.Log("Spreading " + this + " to " + to + " initializing with " + initialLevel);
        lock (BlockThread.actionLock)
        {
            Liquid water = new Water(to, true, initialLevel);
            to.getChunk().setBlock(to, water);
            return water;
        }
    }
}

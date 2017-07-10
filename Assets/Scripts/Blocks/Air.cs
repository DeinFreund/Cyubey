using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class AirData : BlockData
{
    public override Block reconstruct(bool serverBlock)
    {
        return new Air(coords, serverBlock);
    }
}

public class Air : Block {


    public new const short ID = -1;

    public Air(Coordinates coords, bool serverBlock) : base(coords, serverBlock, new AirData())
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

    public override void applyAction(BlockAction action)
    {
        throw new NotImplementedException();
    }
}

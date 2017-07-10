using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class NoBlockData : BlockData
{
    public override Block reconstruct(bool serverBlock)
    {
        return NoBlock.noblock;
    }
}

public class NoBlock : Block
{

    public static Block noblock = new NoBlock(new Coordinates(), false);
    public new const short ID = -1;
    

    public NoBlock(Coordinates coords, bool serverBlock) : base(coords, serverBlock, new NoBlockData())
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

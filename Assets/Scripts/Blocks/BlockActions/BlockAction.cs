using UnityEngine;
using System.Collections;
using System;

public abstract class BlockAction
{
    public readonly Block target;

    private readonly DateTime creationTime;
    protected readonly int hash;

    public BlockAction(Block target, int hash) 
    {
        this.target = target;
        this.creationTime = DateTime.Now;
        this.hash = hash;
    }

    public float getAge()
    {
        return (float)DateTime.Now.Subtract(creationTime).TotalSeconds;
    }

    public override bool Equals(object obj)
    {
        BlockAction o = obj as BlockAction;
        return o != null && target.Equals(o.target);
    }

    public override int GetHashCode()
    {
        return hash;
    }

    public override string ToString()
    {
        return base.ToString() + " affecting " + target;
    }
}

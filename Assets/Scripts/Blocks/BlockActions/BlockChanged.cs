using UnityEngine;
using System.Collections;
using System;

public class BlockChanged : BlockAction
{
    public readonly Block changed;

    public BlockChanged(Block changed, Block target) : base(target, new { target, changed }.GetHashCode()) {
        this.changed = changed;
    }

    public override bool Equals(object obj)
    {
        BlockChanged o = obj as BlockChanged;
        return o != null && target.Equals(o.target) && changed.Equals(o.changed);
    }

    public override string ToString()
    {
        return base.ToString() + " neighbour changed: " + changed;
    }
}

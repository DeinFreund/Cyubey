using UnityEngine;
using System.Collections;
using System;

public class UpdatePressure : BlockAction
{
    public readonly Block changed;

    public UpdatePressure(Block changed, Block target) : base(target, new { target, changed }.GetHashCode()) {
        this.changed = changed;
    }

    public override bool Equals(object obj)
    {
        UpdatePressure o = obj as UpdatePressure;
        return o != null && target.Equals(o.target) && changed.Equals(o.changed);
    }

    public override string ToString()
    {
        return base.ToString() + " being pressured from " + changed;
    }
}

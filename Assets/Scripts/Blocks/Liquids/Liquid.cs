using UnityEngine;
using System.Collections;
using System;

[Serializable]
public abstract class LiquidData : BlockData
{
    public float pressure;
}

public abstract class Liquid : Block
{
    protected new LiquidData data { get { return base.data as LiquidData; } }

    public new const short ID = 0;

    private int meshID = 4;

    public readonly float density = 1;
    private Position pos;
    private DateTime lastPressureUpdate = DateTime.Now;
    
    const float MIN_LEVEL = 0.05f;
    const float MIN_CHANGE_PER_SECOND = 0.01f;
    const float FLOW_RATE = 0.5f;

    public float level
    {
        get
        {
            return Math.Min(1, getPressure());
        }
    }

    public Liquid(Coordinates coords, bool serverBlock, LiquidData data, float initialPressure) : base(coords, serverBlock, data)
    {
        pos = coords;
        if (initialPressure > MIN_LEVEL)
        {
            setPressure(initialPressure);
        }
        else
        {
            data.pressure = 0;
        }
        if (isServerBlock) Debug.Log("On server created " + this);
    }


    public float getPressure()
    {
        if (data.pressure < MIN_LEVEL)
        {
            if (data.pressure > 0)
            {
                Debug.LogWarning("Lost water " + data.pressure + " in " + this);
            }
            data.pressure = 0;
        }
        return data.pressure;
    }

    public void setPressure(float value)
    {
        if (!isServerBlock)
        {
            data.pressure = value;
            return;
        }
        lock (BlockThread.actionLock)
        {
            data.pressure = value;
            lastPressureUpdate = DateTime.Now;
            if (level < MIN_LEVEL)
            {
                if (data.pressure > 0)
                {
                    Debug.LogWarning("Lost water " + data.pressure + " in " + this);
                }
                Debug.Log("blu");
                pos.getChunk().setBlock(pos, new Air(coords, true));
            }
            else
            {
                pos.getChunk().blockUpdated(this, true);
            }
            foreach (Position x in getPosition().getNeighbours())
            {

                Block block = x.getBlock();
                if (block is Air)
                {
                    if (level < MIN_LEVEL) continue;
                    block = spreadTo(x);
                }
                BlockThread.queueAction(new UpdatePressure(this, block));
            };
        }
    }

    public void addPressure(float amt)
    {
        setPressure(getPressure() + amt);
    }

    public override short getMeshID(Coordinates coords)
    {
        return 4;
    }

    public abstract Transform getPrefab();

    protected abstract Block spreadTo(Position to);


    public override short getID()
    {
        return ID;
    }


    public override void applyAction(BlockAction action)
    {
        if (!isServerBlock) return;
        UpdatePressure pu = action as UpdatePressure;
        if (pu != null)
        {
            Debug.Log("Applying pressure to " + this);
            Liquid liq = pu.changed as Liquid;
            if (liq != null)
            {
                float otherPressure = liq.getPressure();
                if (liq.coords.y > coords.y)
                {
                    otherPressure += liq.density;
                }
                if (liq.coords.y < coords.y)
                {
                    otherPressure -= liq.density;
                }
                float dt = Math.Min((float)DateTime.Now.Subtract(lastPressureUpdate).TotalSeconds, pu.getAge());
                float delta = Math.Min(0.1f, FLOW_RATE * dt) * (otherPressure - getPressure());
                delta = Math.Min(otherPressure - MIN_LEVEL, delta);
                if (delta > 0 && getPressure() + delta < MIN_LEVEL)
                {
                    delta = MIN_LEVEL - getPressure() + 0.001f;
                    if (otherPressure - delta < MIN_LEVEL) delta = 0;
                }
                delta = Math.Max(-getPressure(), Math.Min(liq.getPressure(), delta));
                if (Math.Abs(delta) / dt > MIN_CHANGE_PER_SECOND)
                {
                    if (delta + getPressure() < MIN_LEVEL) delta = -getPressure();
                    if (-delta + liq.getPressure() < MIN_LEVEL) delta = liq.getPressure();
                    addPressure(delta);
                    liq.addPressure(-delta);
                    Debug.Log(this + ": Flown " + delta + " - " + this + " is now " + getPressure() + " " + liq + " is now " + liq.getPressure());
                }
                else if (level < MIN_LEVEL)
                {
                    if (data.pressure > 0)
                    {
                        Debug.LogWarning("Lost water " + data.pressure + " in " + this);
                    }
                    Debug.Log(this + ": Removed placeholder");
                    pos.getChunk().setBlock(pos, new Air(coords, true));
                }
                else
                {
                    //Debug.Log(this + ": " + delta + " / " + dt + " too small for change");
                }
            }
        }
        else
        {
            Debug.LogError("Unknown action: " + action);
        }
    }

    public override Field unload()
    {
        return null;
    }
}

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
        if (initialPressure >= MIN_LEVEL)
        {
            setPressure(initialPressure);
        }
        else
        {
            Debug.LogError("Empty water");
            data.pressure = 0;
        }
        if (isServerBlock)
        {
            Debug.Log("On server created " + this);
            Chunk.addNeighbourChangeListener(this);

        }
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
                pos.getChunk().setBlock(pos, new Air(coords, true));
            }
            else
            {
                pos.getChunk().blockUpdated(this, true);
            }
            foreach (Position x in getPosition().getNeighbours())
            {

                Block block = x.getBlock();
                if (block is Solid)
                {
                    BlockThread.queueAction(new UpdatePressure(this, block));
                }
                else if (block is Air)
                {
                    BlockThread.queueAction(new BlockChanged(block, this));
                }
            }
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

    protected abstract Liquid spreadTo(Position to, float initialLevel);


    public override short getID()
    {
        return ID;
    }


    public override void applyAction(BlockAction action)
    {
        if (!isServerBlock) return;
        BlockChanged bc = action as BlockChanged;
        if (bc != null)
        {
            //Debug.Log("Applying " + action);
            Liquid liq = bc.changed as Liquid;
            if (liq != null || bc.changed is Air)
            {

                float otherPressure = liq != null ? liq.getPressure() : 0;
                float otherLvl = otherPressure;
                if (bc.changed.getCoordinates().y > coords.y)
                {
                    otherPressure += density;
                }
                if (bc.changed.getCoordinates().y < coords.y)
                {
                    otherPressure -= density;
                }
                float dt = Math.Min((float)DateTime.Now.Subtract(lastPressureUpdate).TotalSeconds, bc.getAge());
                float delta = Math.Min(0.1f, FLOW_RATE * dt) * (otherPressure - getPressure());
                delta = Math.Max(-(getPressure() - MIN_LEVEL), delta);
                if (delta < 0 && otherLvl - delta <= MIN_LEVEL)
                {
                    delta = -(MIN_LEVEL - otherLvl + 0.001f);
                    if (getPressure() + delta < MIN_LEVEL && coords.y <= bc.changed.getCoordinates().y) delta = 0;
                }
                delta = Math.Max(-getPressure(), Math.Min(otherLvl, delta));
                if (Math.Abs(delta) / dt > MIN_CHANGE_PER_SECOND)
                {
                    //Debug.Log("Trying to flow " + delta);
                    if (delta + getPressure() < MIN_LEVEL) delta = -getPressure();
                    if (-delta + otherLvl < MIN_LEVEL) delta = otherLvl;
                    addPressure(delta);
                    if (liq != null)
                    {
                        liq.addPressure(-delta);
                    }
                    else
                    {
                        liq = spreadTo(bc.changed.getPosition(), -delta) as Liquid;
                    }
                    Debug.Log(this + ": Flown " + delta + " - " + this + " is now " + getPressure() + " " + liq + " is now " + liq.getPressure());
                    if (level < MIN_LEVEL && level > 0.001f)
                    {
                        Debug.LogError("Lost water");
                    }
                }
                else if (level < MIN_LEVEL)
                {
                    if (data.pressure > 0)
                    {
                        Debug.LogError("Lost water " + data.pressure + " in " + this);
                    }
                    //Debug.Log(this + ": Removed placeholder");
                    pos.getChunk().setBlock(pos, new Air(coords, true));
                }
                else
                {
                    //Debug.Log(this + ": " + delta + " / " + dt + " too small for change");
                }
            }
            else
            {
                //Debug.Log("Invalid neighbour");
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

using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class BlockThread
{

    //private static ConcurrentHashSet<Block> blocks = new ConcurrentHashSet<Block>();
    private static ConcurrentQueue<BlockAction> actions = new ConcurrentQueue<BlockAction>();
    private static HashSet<BlockAction> actionSet = new HashSet<BlockAction>();

    static BlockThread()
    {
        Thread worker = new Thread(Run);
        worker.Start();
    }

    public static object actionLock = new object();

    const float MIN_AGE = 0.1f;


    public static void queueAction(BlockAction action)
    {
        //Debug.Log("Queued " + action);
        lock (actionSet)
        {
            if (actionSet.Contains(action)) return;
            actions.Enqueue(action);
            actionSet.Add(action);
            Debug.LogWarning("Added " + action + " target is now " + action.target.getPosition().getBlock());
        }
    }

    /*
    public static void addBlock(Block block)
    {
        blocks.Add(block);
    }

    public static void removeBlock(Block block)
    {
        if (!blocks.Contains(block))
        {
            Debug.LogWarning("Removed unknown block from BlockThread: " + block);
        }
        else
        {
            blocks.Remove(block);
        }
    }*/
    

    private static bool running = true;

    private static void Run()
    {
        while (running) {
            BlockAction action = null;
            lock (actionSet) {
                if (actions.TryDequeue(out action))
                {
                    actionSet.Remove(action);
                }
            }
            if (action != null)
            {
                if (action.getAge() < MIN_AGE)
                {
                    Thread.Sleep(Math.Max(1, (int)(1000 * (MIN_AGE - action.getAge()))));
                }
                if (!action.target.getPosition().getBlock().Equals(action.target))
                {
                    Debug.LogWarning("Scrapped " + action + " target is now " + action.target.getPosition().getBlock());
                }
                else
                {
                    lock (actionLock)
                    {
                        action.target.applyAction(action);
                    }
                }
            }else
            {
                Thread.Sleep(100);
            }
            //Thread.Sleep(2000);
        }
    }

    public static void shutdown()
    {
        running = false;
    }
}

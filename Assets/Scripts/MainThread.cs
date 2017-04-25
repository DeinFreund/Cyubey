﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThread : MonoBehaviour {

    private static ConcurrentQueue<Chunk> chunksAwaitingInstantiation = new ConcurrentQueue<Chunk>();
    private static ConcurrentQueue<Action> events = new ConcurrentQueue<Action>();
    private static ConcurrentQueue<Action> eventsSoon = new ConcurrentQueue<Action>();

    public static void instantiateChunk(Chunk chunk)
    {
        chunksAwaitingInstantiation.Enqueue(chunk);
    }
    public static void runAction(Action action)
    {
        events.Enqueue(action);
    }

    public static void runSoon(Action action)
    {
        eventsSoon.Enqueue(action);
    }

    // Use this for initialization
    void Start () {
		
	}

    float lastSoon = 0;
	// Update is called once per frame
	void Update () {

        Action action;
        while (events.TryDequeue(out action))
        {
            action.Invoke();
        }
        Chunk chunk;
        while (chunksAwaitingInstantiation.TryDequeue(out chunk))
        {
            chunk.setInstantiated(true);
        }
        if (Time.time - lastSoon > 10)
        {
            lastSoon = Time.time;
            while (eventsSoon.TryDequeue(out action))
            {
                action.Invoke();
            }
        }
    }
}

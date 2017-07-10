using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BackgroundThread : MonoBehaviour {
    
    private static ConcurrentQueue<Action> events = new ConcurrentQueue<Action>();

    static BackgroundThread()
    {
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            Thread worker = new Thread(Run);
            //worker.Priority = System.Threading.ThreadPriority.Lowest;
            worker.Start();
        }
    }

    public static void runAction(Action action)
    {
        events.Enqueue(action);
    }
    

    private static bool running = true;

    private static void Run()
    {
        while (running)
        {
            Action action;
            if (events.TryDequeue(out action))
            {
                action.Invoke();
            }else
            {
                Thread.Sleep(100);
            }
        }
    }

    public static void shutdown()
    {
        running = false;
    }
    
}

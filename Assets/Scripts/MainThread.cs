using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThread : MonoBehaviour {

    public static ConcurrentQueue<Action> events = new ConcurrentQueue<Action>();

    public static void runAction(Action action)
    {
        events.Enqueue(action);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        Action action;
        while (events.TryDequeue(out action))
        {
            action.Invoke();
        }
    }
}

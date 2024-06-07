using System.Collections.Generic;
using UnityEngine;
using System;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executeOnMainThread = new Queue<Action>();
    private static MainThreadDispatcher instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static void ExecuteInUpdate(Action action)
    {
        lock (executeOnMainThread)
        {
            executeOnMainThread.Enqueue(action);
        }
    }

    void Update()
    {
        lock (executeOnMainThread)
        {
            while (executeOnMainThread.Count > 0)
            {
                Action action;
                lock (executeOnMainThread)
                {
                    action = executeOnMainThread.Dequeue();
                }
                action();
            }
        }
    }
}

using System;
using System.Collections.Generic;

public class Dispatcher : IDispatcher
{
    public List<Action> pending = new List<Action>();
    private static Dispatcher instance;

    public static Dispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                // Instance singleton on first use.
                instance = new Dispatcher();
            }
            return instance;
        }
    }
    //

    // Schedule code for execution in the main-thread.
    //
    public void Invoke(Action fn)
    {
        lock(pending)
        {
            pending.Add(fn);
        }
    }

    //
    // Execute pending actions.
    //
    public void InvokePending()
    {
        lock (pending)
        {
            foreach (var action in pending)
            {
                action(); // Invoke the action.
            }

            pending.Clear(); // Clear the pending list.
        }
    }
}
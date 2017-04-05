using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ConcurrentQueue<T> : IEnumerable<T>
{
    private Queue<T> m_queue;

    internal int Count
    {
        get { lock (m_queue) { return m_queue.Count; } }
    }

    public ConcurrentQueue()
    {
        m_queue = new Queue<T>();
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (m_queue)
        {
            return m_queue.ToList().GetEnumerator();
        }
    }

    internal bool Contains(T thingie)
    {
        lock (m_queue)
        {
            return m_queue.Contains(thingie);
        }
    }

    internal void Enqueue(T state)
    {
        lock (m_queue)
        {
            m_queue.Enqueue(state);
        }
    }

    internal bool TryDequeue(out T state)
    {
        lock (m_queue)
        {
            if (m_queue.Count > 0)
            {
                state = m_queue.Dequeue();
                return true;
            }
            else
            {
                state = default(T);
                return false;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
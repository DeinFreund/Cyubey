﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine.Profiling;

public class ChunkManager
{

    private int size;
    private string path;
    private static readonly int threads = Environment.ProcessorCount; //pls don't hyperthread

    private static ConcurrentQueue<FSTask> tasks = new ConcurrentQueue<FSTask>();
    private static ChunkManager singleton = new ChunkManager(16, "saves");

    static ChunkManager() {
        for (int i = 0; i < threads; i++)
        {
            Thread worker = new Thread(ProcessTasks);
            worker.Start();
        }
    }

    //path has to be without trailing slash
    private ChunkManager(int size, string path)
    {
        this.size = size;
        this.path = path;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static DateTime startTime;
    private static int loaded = 0;

    private static void ProcessTasks()
    {
        while (true)
        {
            try
            {
                FSTask task;
                if (tasks.TryDequeue(out task))
                {
                    if (loaded++ == 0)
                    {
                        startTime = DateTime.Now;
                    }
                    if (loaded == 200)
                    {
                        Debug.Log("Loaded first 200 chunks in " + DateTime.Now.Subtract(startTime));
                    }
                    if (task.chunkdata != null)
                    {
                        singleton.SaveChunk(task.chunk, task.chunkdata);
                    }else if (!task.chunk.isLoaded())
                    {
                        singleton.LoadChunk(task.chunk);
                    }
                }
                else
                {
                    Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in ProcessTasks: " + ex);
            }
        }
    }

    public static void RequestChunkSave(Chunk chunk, byte[] data)
    {
        //Can be later optimized to batch saves of nearby chunks
        tasks.Enqueue(new FSTask(chunk, data));
    }

    private void SaveChunk(Chunk chunk, byte[] chunkdata)
    {


        int bx = div_floor(chunk.getX(), size);
        int by = div_floor(chunk.getY(), size);
        int bz = div_floor(chunk.getZ(), size);
        string file = path + "/" + bx + "_" + by + "_" + bz + ".region";

        int pos = ((chunk.getX() % size + size) % size) + (size * ((chunk.getY() % size + size) % size)) + (size * size * ((chunk.getZ() % size + size) % size));
        byte[] prev;
        byte[] result;
        int space = 0;
        int oldLength;
        int newLength = chunkdata.Length;

        if (!File.Exists(file))
        {
            result = new byte[(size * size * size * 4) + newLength];

            space = 4 * pos;
        }
        else
        {
            prev = File.ReadAllBytes(file);

            for (int i = 0; i < pos; i++)
            {
                space += 4 + BitConverter.ToInt32(prev, space);
            }

            oldLength = BitConverter.ToInt32(prev, space);
            result = new byte[prev.Length + newLength - oldLength];

            for (int i = 0; i < space; i++)
            {
                result[i] = prev[i];
            }

            for (int i = space + 4 + newLength; i < result.Length; i++)
            {
                result[i] = prev[i - newLength + oldLength];
            }
        }

        Debug.Log("Insert " + chunkdata.Length + " Bytes at Byte " + space);

        for (int i = 0; i < 4; i++)
        {
            result[space + i] = BitConverter.GetBytes(newLength)[i];
        }

        for (int i = 0; i < chunkdata.Length; i++)
        {
            result[space + i + 4] = chunkdata[i];
        }

        File.WriteAllBytes(file, result);
    }

    public static void RequestChunkLoad(Chunk chunk)
    {
        //Can be optimized to load nearby chunks as well to reduce FS load
        tasks.Enqueue(new FSTask(chunk, null));
    }
    
    private static byte[] buffer = new byte[4096];
    private static byte[] intbuf = new byte[4];

    private void LoadChunk(Chunk chunk)
    {
        /*
        MainThread.runAction(() =>
        {
        Profiler.BeginSample("LoadChunk");
        //*/

        int bx = div_floor(chunk.getX(), size);
        int by = div_floor(chunk.getY(), size);
        int bz = div_floor(chunk.getZ(), size);
        string file = path + "/" + bx + "_" + by + "_" + bz + ".region";
        if (!File.Exists(file))
        {
            chunk.deserialize(buffer, 0);
            return;
        } //Region does not exist

        using (Stream source = File.OpenRead(file))
        {
            int chunkLength;
            Chunk c;
            for (int pos = 0; pos < size * size * size; pos++)
            {
                source.Read(intbuf, 0, intbuf.Length);
                chunkLength = BitConverter.ToInt32(intbuf, 0);
                source.Read(buffer, 0, chunkLength);
                c = World.getChunk(new Coordinates(pos % size, (pos / size) % size, pos / (size * size)));
                if (c != null && !c.isLoaded()) c.deserialize(buffer, chunkLength);
            }
        }
            /*
            Profiler.EndSample();
        });
        while (!chunk.isLoaded()) Thread.Sleep(50);
        //*/
    }


    private static int div_floor(int x, int y)
    {
        int q = x / y;
        int r = x % y;
        if ((r != 0) && ((r < 0) != (y < 0))) --q;
        return q;
    }

    private struct FSTask
    {
        public readonly byte[] chunkdata;
        public readonly Chunk chunk;

        public FSTask(Chunk chunk, byte[] data)
        {
            this.chunk = chunk;
            chunkdata = data;
        }
    }
}

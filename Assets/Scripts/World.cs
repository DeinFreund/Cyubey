using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.CompilerServices;

public class World : MonoBehaviour {

    static readonly LocationFreer terrain = new LocationFreer(new Perlin(42, null));
    protected static Dictionary<Coordinates, Chunk> chunks = new Dictionary<Coordinates, Chunk>();

    // Use this for initialization
    void Start() {
        terrain.freeLocation(new Coordinates(0,0,0),4, 12);
        Thread thread = new Thread(generateChunks);
        thread.Start();
    }

    static Queue<Coordinates> loadSoon = new Queue<Coordinates>();
    static Queue<Chunk> initQueue = new Queue<Chunk>();
    static Queue<Coordinates> loadAsap = new Queue<Coordinates>();

    float lastChunkCheck = -2;
    float lastLoadSoon = -2;
    static int minViewDistance = 2;
    static int maxViewDistance = 10;
    // Update is called once per frame
    void Update()
    {
        if (initQueue.Count > 0)
        {
            lastLoadSoon = Time.time;
            Chunk chunk = initQueue.Dequeue();
            if (!chunks.ContainsKey(chunk.getCoordinates()))
            {
                chunks.Add(chunk.getCoordinates(), chunk);
                chunk.init();
            }
        }
        if (Time.time - lastChunkCheck > 0.5)
        {
            Vector3 campos = Camera.main.transform.position;
            Coordinates playerPos = new Position((int)System.Math.Ceiling(campos.x), (int)System.Math.Ceiling(campos.y), (int)System.Math.Ceiling(campos.z) -1).getChunkCoordinates();
            lastChunkCheck = Time.time;
            HashSet<Coordinates> toUnload = new HashSet<Coordinates>();
            foreach (KeyValuePair<Coordinates, Chunk> pair in chunks)
            {
                Chunk chunk = pair.Value;
                if (chunk.getCoordinates().distanceTo(playerPos) > maxViewDistance + 1)
                {
                    toUnload.Add(pair.Key);
                }
            }
            foreach (Coordinates coords in toUnload) {
                unloadChunk(coords);
            }

            //HashSet<Coordinates> loaded = new HashSet<Coordinates>();
            for (int x = playerPos.getX() - minViewDistance; x <= playerPos.getX() + minViewDistance; x++)
            {
                for (int y = playerPos.getY() - minViewDistance; y <= playerPos.getY() + minViewDistance; y++)
                {
                    for (int z = playerPos.getZ() - minViewDistance; z <= playerPos.getZ() + minViewDistance; z++)
                    {
                        Coordinates coords = new Coordinates(x, y, z);
                        if (coords.distanceTo(playerPos) <= minViewDistance + 0.0001f && !chunks.ContainsKey(coords))
                        {
                            loadChunk(coords, false);
                            //return; //one chunk at a time
                            //loaded.Add(coords);
                        }
                    }
                }
            }
        }
    }

    private void generateChunks()
    {
        while (true)
        {
            if (loadSoon.Count > 0)
            {
                loadChunk(loadSoon.Dequeue(), true);
            }else
            {
                Thread.Sleep(10);
            }
        }
    }
    
    public static int getMinViewDistance()
    {
        return minViewDistance;
    }
    public static int getMaxViewDistance()
    {
        return maxViewDistance;
    }
    public static void requestChunkLoad(Coordinates coords)
    {
        if (!loadSoon.Contains(coords) && !chunks.ContainsKey(coords))
        loadSoon.Enqueue(coords);
    }
    public static int getRequestCount()
    {
        return loadSoon.Count;
    }
    public static void forceChunkLoad(Coordinates coords)
    {
        if (!loadAsap.Contains(coords) && !chunks.ContainsKey(coords))
            loadAsap.Enqueue(coords);
    }

    public static Chunk getChunk(Coordinates coords)
    {
        
        if (!chunks.ContainsKey(coords))
        {
            return null;
        }
        return chunks[coords];
    }

    public static Dictionary<Coordinates, Chunk>.ValueCollection getChunks()
    {
        return chunks.Values;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void loadChunk(Coordinates coords, bool slow)
    {
        if (chunks.ContainsKey(coords)) return;
        foreach (Chunk c in initQueue)
        {
            if (c.getCoordinates().Equals(coords)) return;
        }
        //Debug.Log("Loading Chunk at " + coords.ToString());
        if (!slow)Profiler.BeginSample("Loading Chunks");
        if (!slow)
        {
            chunks.Add(coords, new Chunk(coords, terrain));
            chunks[coords].init();
        }
        else {
            initQueue.Enqueue(new Chunk(coords, terrain));
        } 
        if (!slow) Profiler.EndSample();
    }

    private static void unloadChunk(Coordinates coords)
    {
        if (!chunks.ContainsKey(coords)) return;
        Profiler.BeginSample("Unloading Chunks");
        //Debug.Log("Unloading Chunk at " + coords.ToString());
        chunks[coords].unload();
        chunks.Remove(coords);
        Profiler.EndSample();
    }

    void OnApplicationQuit()
    {
        if (NetworkManager.isServer())
        {
            Account.saveAccounts();
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.CompilerServices;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Profiling;
using System.Linq;

public class World : MonoBehaviour {

    protected static Dictionary<Coordinates, Chunk> chunks = new Dictionary<Coordinates, Chunk>();
    
    void Awake()
    {
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Chunk).TypeHandle);
    }

    // Use this for initialization
    void Start() {
        curTime = lastAlive = Time.time;
        Thread thread = new Thread(generateChunks);
        thread.Start();
        ClientNetworkManager.send(TCPMessageID.READY, new Field());
    }

    static ConcurrentQueue<Coordinates> loadSoon = new ConcurrentQueue<Coordinates>();
    static ConcurrentQueue<Coordinates> loadAsap = new ConcurrentQueue<Coordinates>();
    static HashSet<Coordinates> loadingChunks = new HashSet<Coordinates>();

    float lastChunkCheck = -2;
    float lastLoadSoon = -2;
    static int minViewDistance = 1;
    static int preloadDistance = 3;
    static int maxViewDistance = 8;
    float lastAlive;
    float curTime;
    // Update is called once per frame
    void Update()
    {
        curTime = Time.time;
        if (Time.time - lastAlive > 2)
        {
            Debug.LogError("Loading thread died");
        }
        if (Camera.main == null) return;
        if (Time.time - lastChunkCheck > 1.0)
        {
            lock (chunks)
            {
                Coordinates playerPos = MovementController.feetPosition.getChunkCoordinates();
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
                foreach (Coordinates coords in toUnload)
                {
                    unloadChunk(coords);
                }
                for (int x = playerPos.getX() - preloadDistance; x <= playerPos.getX() + preloadDistance; x++)
                {
                    for (int y = playerPos.getY() - preloadDistance; y <= playerPos.getY() + preloadDistance; y++)
                    {
                        for (int z = playerPos.getZ() - preloadDistance; z <= playerPos.getZ() + preloadDistance; z++)
                        {
                            Coordinates coords = new Coordinates(x, y, z);
                            if (coords.distanceTo(playerPos) <= preloadDistance + 0.0001f && !chunks.ContainsKey(coords))
                            {
                                requestChunkLoad(coords);
                            }
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
            try
            {
                //Debug.Log(loadSoon.Count);
                lastAlive = curTime;
                Coordinates toLoad;
                if (loadSoon.TryDequeue(out toLoad))
                {
                    loadChunk(toLoad);
                }
                else
                {
                    Thread.Sleep(30);
                }
            } catch (Exception ex)
            {
                Debug.LogError("Exception in generateChunks: " + ex);
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
    public static void registerChunk(Coordinates coords, Chunk chunk)
    {
        lock (chunks)
        {
            loadingChunks.Remove(coords);
            chunks[coords] = chunk;
        }
    }
    public static void requestChunkLoad(Coordinates coords)
    {
        if (!loadSoon.Contains(coords) && !chunks.ContainsKey(coords))
        {
            //Debug.Log("Loading soon " + coords);
            loadSoon.Enqueue(coords);
        }
    }
    public static int getRequestCount()
    {
        return loadSoon.Count;
    }
    public static void forceChunkLoad(Coordinates coords)
    {
        lock (chunks)
        {
            if (!loadAsap.Contains(coords) && !chunks.ContainsKey(coords))
                loadAsap.Enqueue(coords);
        }
    }

    public static Chunk getChunk(Coordinates coords)
    {
        return getChunk(coords, false);
    }

    public static Chunk getChunk(Coordinates coords, bool unloaded)
    {
        lock (chunks)
        {
            if (!chunks.ContainsKey(coords) || !unloaded && loadingChunks.Contains(coords))
            {
                return null;
            }
            return chunks[coords];
        }
    }
    
    //needs locking!
    public static Dictionary<Coordinates, Chunk> getChunks()
    {
        return chunks;
    }
    
    private static void loadChunk(Coordinates coords)
    {
        lock (chunks)
        {
            if (chunks.ContainsKey(coords)) return;
            if (loadingChunks.Contains(coords)) return;
            loadingChunks.Add(coords);
            chunks[coords] = new Chunk(coords);
            TerrainCompositor.ChunkLoaded(chunks[coords]);
        }
    }

    private static void unloadChunk(Coordinates coords)
    {
        lock (chunks)
        {
            if (!chunks.ContainsKey(coords)) return;
            Profiler.BeginSample("Unloading Chunks");
            //Debug.Log("Unloading Chunk at " + coords.ToString());
            TerrainCompositor.ChunkUnloaded(chunks[coords]);
            chunks[coords].unload();
            chunks.Remove(coords);
            Profiler.EndSample();
        }
    }

    void OnApplicationQuit()
    {
        lock (chunks)
        {
            if (ServerNetworkManager.isServer())
            {
                Account.saveAccounts();
                ServerNetworkManager.shutdown();
            }
            ClientNetworkManager.shutdown();
            foreach (Chunk c in chunks.Values)
            {
                c.unload();
            }
        }
    }
}

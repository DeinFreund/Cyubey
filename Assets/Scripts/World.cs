using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.CompilerServices;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Profiling;

public class World : MonoBehaviour {

    static readonly LocationFreer terrain = new LocationFreer(
        new LayerBlender(
            new Layer(new Perlin(42, new float[]{0.01f, 0.05f, 0.1f, 0.5f}, 1), -1),
            new Layer(new Perlin(42, new float[] { 0.01f, 0.05f, 0.1f, 0.5f }, 4), 0)
        ));
    protected static Dictionary<Coordinates, Chunk> chunks = new Dictionary<Coordinates, Chunk>();

    // Use this for initialization
    void Start() {
        terrain.freeLocation(new Coordinates(0,0,0),4, 12);
        curTime = lastAlive = Time.time;
        Thread thread = new Thread(generateChunks);
        thread.Start();
        ClientNetworkManager.send(TCPMessageID.READY, new Field());
    }

    static ConcurrentQueue<Coordinates> loadSoon = new ConcurrentQueue<Coordinates>();
    static ConcurrentQueue<Chunk> initQueue = new ConcurrentQueue<Chunk>();
    static ConcurrentQueue<Coordinates> loadAsap = new ConcurrentQueue<Coordinates>();

    float lastChunkCheck = -2;
    float lastLoadSoon = -2;
    static int minViewDistance = 1;
    static int preloadDistance = 3;
    static int maxViewDistance = 10;
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
        Chunk toInit;
        if (initQueue.TryDequeue(out toInit))
        {
            lastLoadSoon = Time.time;
            if (!chunks.ContainsKey(toInit.getCoordinates()))
            {
                chunks.Add(toInit.getCoordinates(), toInit);
                toInit.init();
            }
        }
        if (Time.time - lastChunkCheck > 1.0)
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
            }//HashSet<Coordinates> loaded = new HashSet<Coordinates>();
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
                    loadChunk(toLoad, true);
                }
                else
                {
                    Thread.Sleep(30);
                }
            }catch(Exception ex)
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
        if (ServerNetworkManager.isServer())
        {
            Account.saveAccounts();
            ServerNetworkManager.shutdown();
        }
        ClientNetworkManager.shutdown();
    }
}

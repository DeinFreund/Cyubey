﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public class Chunk
{
    public const int size = 16;

    protected static HashSet<Block> blockChangeListeners = new HashSet<Block>();

    protected int x, y, z;
    protected Coordinates coords;
    protected List<Face> faces = new List<Face>();
    protected Block[,,] blocks = null;
    protected BitArray[,] blocksModified = null;
    protected bool untouchedChunk = true;
    protected GameObject empty;
    protected bool modifiedSinceSave = false;
    protected Dictionary<Transform, GameObject> fluids = new Dictionary<Transform, GameObject>();
    protected readonly Position centerpos;
    protected bool terrainReady;
    protected bool deserialized;
    protected bool faceRecalculationNeeded = true;
    protected bool terrainMeshRebuildingNeeded = true;
    protected bool fluidMeshRebuildingNeeded = true;

    protected bool loaded = false; //has been deserialized
    protected ConcurrentQueue<KeyValuePair<Position, Block>> setBlockQueue = new ConcurrentQueue<KeyValuePair<Position, Block>>();

    public Chunk(Coordinates coords)
    {

        //Profiler.BeginSample("Constructing chunk");
        this.x = coords.getX();
        this.y = coords.getY();
        this.z = coords.getZ();
        this.coords = new Coordinates(x, y, z);
        this.centerpos = new Position(size / 2, size / 2, size / 2, this);
        terrainReady = TerrainCompositor.GetBlockReady(centerpos);

        generate();
        faces.Add(new Face(this, new Coordinates(0, 1, 0)));
        faces.Add(new Face(this, new Coordinates(0, -1, 0)));
        faces.Add(new Face(this, new Coordinates(1, 0, 0)));
        faces.Add(new Face(this, new Coordinates(-1, 0, 0)));
        faces.Add(new Face(this, new Coordinates(0, 0, 1)));
        faces.Add(new Face(this, new Coordinates(0, 0, -1)));
        //Profiler.EndSample();
    }

    public void generate()
    {
        if (!ServerNetworkManager.isServer())
        {
            ClientNetworkManager.requestChunk(getCoordinates());
        }
        else
        {
            ChunkManager.RequestChunkLoad(this);
        }
    }


    public void deserialize(byte[] data, int length)
    {
        if (blocks == null) blocks = new Block[size, size, size];
        lock (blocks)
        {
            //Profiler.BeginSample("Loading chunks");
            blocksModified = ChunkSerializer.deserializeBlocks(blocks, data, length, ServerNetworkManager.isServer());
            deserialized = true;
            untouchedChunk = length == 0;
            finishInitialization();
        }
    }

    protected void finishInitialization()
    {
        lock (blocks)
        {
            if (!terrainReady) return;
            if (loaded) return;
            int x, y, z;
            bool isServer = ServerNetworkManager.isServer();
            List<Liquid> liquids = new List<Liquid>();
            for (x = 0; x < size; x++)
            {
                for (y = 0; y < size; y++)
                {
                    if (blocksModified == null || blocksModified[x, y] == null)
                    {
                        for (z = 0; z < size; z++)
                        {
                            blocks[x, y, z] = BlockFactory.create(new Coordinates(x + size * this.x, y + size * this.y, z + size * this.z), isServer, liquids);
                        }
                    }
                    else if (blocksModified[x, y] != null)
                    {
                        for (z = 0; z < size; z++)
                        {
                            if (!blocksModified[x, y][z])
                            {
                                blocks[x, y, z] = BlockFactory.create(new Coordinates(x + size * this.x, y + size * this.y, z + size * this.z), isServer, liquids);
                            }
                        }
                    }
                }
            }

            foreach (Liquid liquid in liquids)
            {
                if (!fluids.ContainsKey(liquid.getPrefab()))
                {
                    fluids.Add(liquid.getPrefab(), null);
                    MainThread.runAction(() => fluids[liquid.getPrefab()] = GameObject.Instantiate(liquid.getPrefab(), empty.transform).gameObject);
                }
            }

            //Debug.Log("Loaded Chunk at " + coords);
            foreach (KeyValuePair<Position, Block> pair in setBlockQueue)
            {
                setBlock(pair.Key, pair.Value, false);
            }
            recalculateFaceConnections();
            MainThread.instantiateChunk(this);
            loaded = true;
            World.registerChunk(getCoordinates(), this);
            //Profiler.EndSample();
        }
    }

    public bool isLoaded()
    {
        return loaded;
    }

    public byte[] serialize()
    {
        return ChunkSerializer.serializeBlocks(blocksModified, blocks);
    }

    public void saved()
    {
        //not really thread safe, hope it works
        modifiedSinceSave = false;
    }

    public bool isModifiedSinceSave()
    {
        return modifiedSinceSave;
    }

    public byte[] hash()
    {
        return ChunkSerializer.hash(serialize());
    }

    public void unload()
    {
        setInstantiated(false);
        foreach (Face f in faces)
        {
            if (f.getOpposingFace() != null) f.getOpposingFace().opposingFace = null;
        }

        blockChangeListeners.RemoveWhere(c => c.getCoordinates().getChunkCoordinates() == getCoordinates());
        ChunkManager.RequestChunkSave(this);
    }

    private static int[] meshes = new int[size * size * size];
    private static float[] meshHeight = new float[size * size * size];
    private static Vector3[] positions = new Vector3[size * size * size];
    private static CombineInstance[] combine = new CombineInstance[size * size * size / 2];
    private static Mesh nothing = new Mesh();
    private static Liquid liquid;
    public void rebuildMesh()
    {
        if (terrainMeshRebuildingNeeded)
        {
            terrainMeshRebuildingNeeded = false;
            Profiler.BeginSample("Building chunk terrain mesh");

            Profiler.BeginSample("Reading meshes");
            int count = 0;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        if (!getBlock(x, y, z).isTransparent() && (getBlock(x, y, z + 1).isTransparent() || getBlock(x, y, z - 1).isTransparent() || getBlock(x, y + 1, z).isTransparent()
                                || getBlock(x, y - 1, z).isTransparent() || getBlock(x + 1, y, z).isTransparent() || getBlock(x - 1, y, z).isTransparent()))
                        {
                            meshes[count] = getBlock(x, y, z).getMeshID();
                            positions[count++] = new Vector3(x, y, z);
                        }
                    }
                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("Combining meshes");
            Matrix4x4 transform = new Matrix4x4();
            Vector4 vec4 = new Vector4();
            for (int i = 0; i < count; i++)
            {
                combine[i].mesh = BlockFactory.blockmesh[meshes[i]];
                vec4.Set(((3 * positions[i].z + 7) % 5 % 2) * 2 - 1, 0, 0, positions[i].x + ((3 * positions[i].z + 7) % 5 % 2) - 1);
                //vec4.Set(-1, 0, 0, positions[i].x + 1);
                //Debug.Log(new Vector4(((3 * i + 7) % 5 % 2) * 2 - 1, 0, 0, positions[i].x + 1 - ((3 * i + 7) % 5 % 2)));
                //vec4.Set(1, 0, 0, positions[i].x);
                transform.SetRow(0, vec4);
                //vec4.Set(0, 1, 0, positions[i].y);
                vec4.Set(0, ((17 * positions[i].x + 11) % 7 % 2) * 2 - 1, 0, positions[i].y + 1 - ((17 * positions[i].x + 11) % 7 % 2));
                transform.SetRow(1, vec4);
                //vec4.Set(0, 0, 1, positions[i].z);
                vec4.Set(0, 0, ((29 * positions[i].x + 97) % 11 % 2) * 2 - 1, positions[i].z + 1 - ((29 * positions[i].x + 97) % 11 % 2));
                transform.SetRow(2, vec4);
                vec4.Set(0, 0, 0, 1);
                transform.SetRow(3, vec4);
                combine[i].transform = transform;
            }
            for (int i = count; i < combine.Length; i++)
            {
                combine[i].mesh = nothing;
            }
            Profiler.EndSample();
            Profiler.BeginSample("Applying mesh");
            empty.GetComponent<MeshFilter>().mesh = new Mesh();
            empty.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            empty.SetActive(false);
            empty.SetActive(true);
            Profiler.EndSample();
            Profiler.EndSample();
        }

        if (fluidMeshRebuildingNeeded)
        {
            fluidMeshRebuildingNeeded = false;
            Matrix4x4 transform = new Matrix4x4();
            Vector4 vec4 = new Vector4();
            foreach (Transform liquidType in fluids.Keys)
            {
                if (fluids[liquidType] == null)
                {
                    Debug.LogWarning("Missing fluid transform, requeuing");
                    fluidMeshRebuildingNeeded = true;
                    MainThread.runAction(() => rebuildMesh());
                    return;
                }
                Profiler.BeginSample("Building chunk fluid mesh");

                Profiler.BeginSample("Reading meshes");
                int count = 0;
                Liquid liq;
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        for (int z = 0; z < size; z++)
                        {
                            liq = getBlock(x, y, z) as Liquid;
                            if (liq != null && liq.getMeshID() >= 0 && liq.level > 0.00001)
                            {
                                meshes[count] = liq.getMeshID();
                                meshHeight[count] = liq.level;
                                positions[count++] = new Vector3(x, y, z);
                            }
                        }
                    }
                }
                Profiler.EndSample();
                Profiler.BeginSample("Combining meshes");
                for (int i = 0; i < count; i++)
                {
                    combine[i].mesh = BlockFactory.blockmesh[meshes[i]];
                    vec4.Set(1, 0, 0, positions[i].x);
                    transform.SetRow(0, vec4);
                    vec4.Set(0, meshHeight[i], 0, positions[i].y);
                    transform.SetRow(1, vec4);
                    vec4.Set(0, 0, 1, positions[i].z);
                    transform.SetRow(2, vec4);
                    vec4.Set(0, 0, 0, 1);
                    transform.SetRow(3, vec4);
                    combine[i].transform = transform;
                }
                for (int i = count; i < combine.Length; i++)
                {
                    combine[i].mesh = nothing;
                }
                Profiler.EndSample();
                Profiler.BeginSample("Applying mesh");
                GameObject empty = fluids[liquidType];
                empty.GetComponent<MeshFilter>().mesh = new Mesh();
                empty.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
                empty.SetActive(false);
                empty.SetActive(true);
                Profiler.EndSample();
                Profiler.EndSample();
            }
        }
    }

    public void TerrainReady()
    {
        this.terrainReady = true;
        finishInitialization();
    }

    private Color debugColor;

    public void setDebugColor(Color color)
    {

        if (!Profiler.enabled || color == debugColor) return;
        debugColor = color;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    getBlock(x, y, z).setDebugColor(color);
                }
            }
        }
    }

    protected bool isRendered = true;

    public void setRendered(bool value)
    {
        //Debug.DrawLine(Camera.main.transform.position, new Position(0, 0, 0, this).offset(size / 2, size / 2, size / 2), value ? Color.green : Color.red, 0.017f);
        //if (value == isRendered) return;

        Profiler.BeginSample("Updating chunk renderers");
        isRendered = value;
        if (empty != null)
        {
            empty.GetComponent<MeshRenderer>().enabled = value;
        }
        foreach (GameObject empty in fluids.Values)
        {
            if (empty != null) empty.GetComponent<MeshRenderer>().enabled = value;
        }
        Profiler.EndSample();
    }

    protected bool isInstantiated = false;

    public void setInstantiated(bool value)
    {
        //Debug.DrawLine(Camera.main.transform.position, new Position(0, 0, 0, this).offset(size / 2, size / 2, size / 2), value ? Color.green : Color.red, 0.017f);
        if (value == isInstantiated) return;


        MainThread.runAction(() =>
        {
            Profiler.BeginSample("Updating chunk instantiation");
            isInstantiated = value;
            if (value)
            {
                empty = ((Transform)GameObject.Instantiate(BlockFactory.chunk, size * coords, Quaternion.identity)).gameObject;
                empty.transform.SetParent(MovementController.worldParent.transform, false);

                rebuildMesh();
                setRendered(false);
            }
            else
            {
                GameObject.Destroy(empty);
                empty = null;
            }

            Profiler.EndSample();
        });
    }

    public int getXOffset()
    {
        return x * size;
    }
    public int getYOffset()
    {
        return y * size;
    }
    public int getZOffset()
    {
        return z * size;
    }
    public Position getCenter()
    {
        return centerpos;
    }
    public int getX()
    {
        return x;
    }
    public int getY()
    {
        return y;
    }
    public int getZ()
    {
        return z;
    }
    public List<Face> getFaces()
    {
        return faces;
    }
    public Face getFaceByNormal(Coordinates normal)
    {
        foreach (Face f in faces)
        {
            if (f.getNormal().Equals(normal))
            {
                return f;
            }
        }
        return null;
    }
    public Coordinates getCoordinates()
    {
        return coords;
    }
    public Block getBlock(int x, int y, int z)
    {
        return (x >= 0 && y >= 0 && z >= 0 && x < size && y < size && z < size) ? blocks[x, y, z] : NoBlock.noblock;
    }

    public static void addNeighbourChangeListener(Block listener)
    {
        lock (blockChangeListeners)
        {
            blockChangeListeners.Add(listener);
        }
    }
    //called by blocks if they have been modified, alerting the chunk that their content has changed
    public void blockUpdated(Block b, bool meshChange)
    {
        modifiedSinceSave = true;
        untouchedChunk = false;
        if (meshChange)
        {
            fluidMeshRebuildingNeeded = fluidMeshRebuildingNeeded || b is Liquid;
            terrainMeshRebuildingNeeded = terrainMeshRebuildingNeeded || b is Solid;
            if (isInstantiated) MainThread.runAction(() => rebuildMesh());
        }

        lock (blockChangeListeners)
        {
            foreach (Position n in b.getPosition().getNeighbours())
            {
                if (blockChangeListeners.Contains(n.getBlock()))
                {
                    BlockThread.queueAction(new BlockChanged(b, n.getBlock()));
                }
            }
        }
    }
    //call if a block in the chunk has been added/removed
    public void setBlock(Position p, Block b)
    {
        setBlock(p, b, true);
    }
    protected void setBlock(Position p, Block b, bool recalculateConnections)
    {
        Debug.Log("set " + p + " to " + b + " from " + p.getBlock() + " in " + this);
        if (!loaded)
        {
            setBlockQueue.Enqueue(new KeyValuePair<Position, Block>(p, b));
            return;
        }


        lock (blocks)
        {
            if (b.getMeshID() >= 0 && b.isTransparent())
            {
                liquid = b as Liquid;
                if (liquid != null && !fluids.ContainsKey(liquid.getPrefab()))
                {
                    fluids.Add(liquid.getPrefab(), null);
                    MainThread.runAction(() => fluids[liquid.getPrefab()] = GameObject.Instantiate(liquid.getPrefab(), empty.transform).gameObject);
                }
            }
            //Debug.Log("Updating block at " + p.getRelativeCoords() + " (" + p + ")");
            ServerNetworkManager.updateBlock(p, b);
            if (blocksModified == null)
            {
                blocksModified = new BitArray[size, size];
            }
            if (blocksModified[p.getRelativeX(), p.getRelativeY()] == null)
            {
                blocksModified[p.getRelativeX(), p.getRelativeY()] = new BitArray(size);
            }
            blocksModified[p.getRelativeX(), p.getRelativeY()][p.getRelativeZ()] = true;
            untouchedChunk = false;
            modifiedSinceSave = true;
            fluidMeshRebuildingNeeded = fluidMeshRebuildingNeeded || blocks[p.getRelativeX(), p.getRelativeY(), p.getRelativeZ()] is Liquid;
            terrainMeshRebuildingNeeded = terrainMeshRebuildingNeeded || blocks[p.getRelativeX(), p.getRelativeY(), p.getRelativeZ()] is Solid;
            lock (blockChangeListeners)
            {
                blockChangeListeners.Remove(blocks[p.getRelativeX(), p.getRelativeY(), p.getRelativeZ()]);
            }
            blocks[p.getRelativeX(), p.getRelativeY(), p.getRelativeZ()] = b;
            blockUpdated(b, true);

            faceRecalculationNeeded = true;
            if (recalculateConnections)
            {
                BackgroundThread.runAction(() => recalculateFaceConnections());
            }

            Debug.Log("now " + p.getBlock() + " | " + b);
        }
    }
    List<Face> _faces;

    private static Stack<int> stk = new Stack<int>(10000);
    public void recalculateFaceConnections()
    {
        if (!faceRecalculationNeeded) return;
        lock (stk)
        {
            if (!faceRecalculationNeeded) return;
            faceRecalculationNeeded = false;
            bool[,,] visited = new bool[size, size, size];
            _faces = new List<Face>();
            foreach (Face f in faces)
            {
                _faces.Add(new Face(f.getChunk(), f.getNormal()));
            }


            //Profiler.BeginSample("Recalculating Face Connections");
            HashSet<Face> faceset = new HashSet<Face>();
            int locX, locY, locZ, i;
            for (int sx = 0; sx < size; sx++)
                for (int sy = 0; sy < size; sy++)
                    for (int sz = 0; sz < size; sz++)
                    {
                        if (visited[sx, sy, sz]) continue;
                        if (!getBlock(sx, sy, sz).isTransparent()) continue;
                        stk.Clear();
                        stk.Push(sx | (sy << 8) | (sz << 16));
                        faceset.Clear();
                        while (stk.Count > 0)
                        {
                            locX = (stk.Peek() & 0x0000FF) >> 0;
                            locY = (stk.Peek() & 0x00FF00) >> 8;
                            locZ = (stk.Peek() & 0xFF0000) >> 16;
                            stk.Pop();

                            if ((locX >= size || locY >= size || locZ >= size) || (locX < 0 || locY < 0 || locZ < 0)) continue;
                            if (visited[locX, locY, locZ]) continue;
                            if (!getBlock(locX, locY, locZ).isTransparent()) continue;

                            visited[locX, locY, locZ] = true;
                            for (i = 0; i < _faces.Count; i++)
                            {
                                if (_faces[i].isFaceBlock(locX, locY, locZ))
                                {
                                    faceset.Add(_faces[i]);
                                }
                                stk.Push((locX + _faces[i].nX) | ((locY + _faces[i].nY) << 8) | ((locZ + _faces[i].nZ) << 16));
                            }
                        }
                        foreach (Face f1 in faceset)
                        {
                            foreach (Face f2 in faceset)
                            {
                                f1.connectedFaces.Add(f2);
                            }
                        }
                    }

            faces = _faces;
        }
        //Profiler.EndSample();


    }
    public class Face
    {
        protected Coordinates normal;
        public int nX, nY, nZ;
        public HashSet<Face> connectedFaces = new HashSet<Face>();
        protected Chunk chunk;
        protected HashSet<Position> faceset = null;


        public Face(Chunk chunk, Coordinates normal)
        {
            this.normal = normal;
            this.nX = normal.getX();
            this.nY = normal.getY();
            this.nZ = normal.getZ();
            this.chunk = chunk;

        }

        public Coordinates getNormal()
        {
            return normal;
        }

        public bool isFaceBlock(int x, int y, int z)
        {
            if (nX + x >= size || nY + y >= size || nZ + z >= size) return true;
            if (nX + x < 0 || nY + y < 0 || nZ + z < 0) return true;
            return false;
        }



        public Chunk getChunk()
        {
            return chunk;
        }

        public HashSet<Position> getAssociatedPositions()
        {
            if (faceset == null)
            {
                faceset = new HashSet<Position>();

                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        for (int z = 0; z < size; z++)
                        {
                            if (isFaceBlock(x, y, z)) faceset.Add(new Position(x, y, z, chunk));
                        }
                    }
                }
            }
            return faceset;
        }

        public HashSet<Face> getConnectedFaces()
        {
            return connectedFaces;
        }


        int cnt = 0;
        int cnt2 = 0;

        public Face opposingFace;

        public Face getOpposingFace()
        {
            if (opposingFace != null) return opposingFace;
            Profiler.BeginSample("getopposingface");
            Chunk next = World.getChunk(chunk.getCoordinates() + getNormal());
            if (next != null)
            {
                opposingFace = next.getFaceByNormal(-1 * getNormal());
                Profiler.EndSample();
                return opposingFace;
            }
            Profiler.EndSample();
            return null;
        }

    }

    public override int GetHashCode()
    {
        return (int)((getX() + (long)getY() * 30011 + (long)getZ() * 1000003) % 1000000007);
    }

    public override bool Equals(System.Object other)
    {
        return other is Chunk && (Equals((Chunk)other));
    }

    public bool Equals(Chunk other)
    {
        return other != null && other.getX() == getX() && other.getY() == getY() && other.getZ() == getZ();
    }

    public override string ToString()
    {
        return "Chunk@" + coords;
    }
}

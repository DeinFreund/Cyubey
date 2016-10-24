using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Chunk
{
    public const int size = 16;

    protected int x, y, z;
    protected Coordinates coords;
    protected IGenerator perlin;
    protected List<Face> faces = new List<Face>();
    protected Block[,,] blocks = null;
    protected GameObject empty;
    protected readonly Position centerpos;

    public Chunk(Coordinates coords, IGenerator perlin)
    {

        //Profiler.BeginSample("Constructing chunk");
        this.x = coords.getX();
        this.y = coords.getY();
        this.z = coords.getZ();
        this.coords = new Coordinates(x, y, z);
        this.perlin = perlin;
        this.centerpos =  new Position(size / 2, size / 2, size / 2, this);
        if (perlin == null) throw new Exception("Perlin is null");

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

        //Profiler.BeginSample("Initializing block array");
        blocks = new Block[size, size, size];
        //Profiler.EndSample();
        //Profiler.BeginSample("Generating chunks");
        int x, y, z;
        for (x = 0; x < size; x++)
        {
            for (y = 0; y < size; y++)
            {
                for (z = 0; z < size; z++)
                {
                    blocks[x,y,z] = BlockFactory.create(new Coordinates(x + size * this.x, y + size * this.y, z + size * this.z), perlin);
                }
            }
        }
        //Profiler.EndSample();
        //Debug.Log("Generated chunk at " + coords.ToString());
    }

    //call after chunk has been constructed
    public void init()
    {

        Profiler.BeginSample("Initializing chunks");
        setInstantiated(true);

        recalculateFaceConnections();
        rebuildMesh();
        Profiler.EndSample();
    }

    public void unload()
    {
        setInstantiated(false);
        foreach (Face f in faces)
        {
            if (f.getOpposingFace() != null) f.getOpposingFace().opposingFace = null;
        }
    }

    private static int[] meshes = new int[size * size * size];
    private static Vector3[] positions = new Vector3[size * size * size];
    private static CombineInstance[] combine = new CombineInstance[size * size * size / 2];
    public void rebuildMesh()
    {

        Profiler.BeginSample("Building chunk mesh");
        
        Profiler.BeginSample("Reading meshes");
        int count = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (getBlock(x, y, z).getMeshID() >= 0 && (getBlock(x, y, z + 1).isTransparent() || getBlock(x, y, z - 1).isTransparent() || getBlock(x, y + 1, z).isTransparent()
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
        Mesh nothing = new Mesh();
        for (int i = 0; i < count; i++)
        {
            combine[i].mesh = BlockFactory.blockmesh[meshes[i]];
            vec4.Set(((3 * i + 7) % 5 % 2) * 2 - 1, 0, 0, positions[i].x + ((3 * i + 7) % 5 % 2) - 1);
            //vec4.Set(-1, 0, 0, positions[i].x + 1);
            //Debug.Log(new Vector4(((3 * i + 7) % 5 % 2) * 2 - 1, 0, 0, positions[i].x + 1 - ((3 * i + 7) % 5 % 2)));
            //vec4.Set(1, 0, 0, positions[i].x);
            transform.SetRow(0, vec4);
            //vec4.Set(0, 1, 0, positions[i].y);
            vec4.Set(0, ((17 * i + 11) % 7 % 2) * 2 - 1, 0, positions[i].y + 1 - ((17 * i + 11) % 7 % 2));
            transform.SetRow(1, vec4);
            //vec4.Set(0, 0, 1, positions[i].z);
            vec4.Set(0, 0, ((29 * i + 97) % 11 % 2) * 2 - 1, positions[i].z + 1 - ((29 * i + 97) % 11 % 2));
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
        //empty.GetComponent<MeshFilter>().mesh.Optimize();
        //empty.GetComponent<MeshCollider>().sharedMesh = empty.GetComponent<MeshFilter>().mesh;
        empty.SetActive(false);
        empty.SetActive(true);
        Profiler.EndSample();
        Profiler.EndSample();
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
        if (value == isRendered) return;

        Profiler.BeginSample("Updating chunk renderers");
        isRendered = value;
        if (empty)
        {
            empty.GetComponent<MeshRenderer>().enabled = value;
        }
        Profiler.EndSample();
    }

    protected bool isInstantiated = false;

    public void setInstantiated(bool value)
    {
        //Debug.DrawLine(Camera.main.transform.position, new Position(0, 0, 0, this).offset(size / 2, size / 2, size / 2), value ? Color.green : Color.red, 0.017f);
        if (value == isInstantiated) return;

        Profiler.BeginSample("Updating chunk instantiation");
        isInstantiated = value;
        if (value)
        {
            empty = ((Transform)UnityEngine.Object.Instantiate(BlockFactory.chunk, size * coords, Quaternion.identity)).gameObject;
        }else
        {
            UnityEngine.Object.Destroy(empty);
            empty = null;
        }

        Profiler.EndSample();
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
        foreach(Face f in faces)
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
    //call if a block in the chunk has been added/removed
    public void setBlock(Position p, Block b)
    {
        blocks[p.getX(), p.getY(), p.getZ()] = b;
        recalculateFaceConnections();
    }
    List<Face> _faces;

    private static Stack<int> stk = new Stack<int>(10000);
    public void recalculateFaceConnections()
    {
        bool[,,] visited = new bool[size, size, size];
        foreach (Face f in faces) {
            f.connectedFaces = new HashSet<Face>();
        }
        _faces = new List<Face>(faces);


        Profiler.BeginSample("Recalculating Face Connections");
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
                        for (i = 0; i < faces.Count; i++)
                        {
                            if (faces[i].isFaceBlock(locX, locY, locZ))
                            {
                                faceset.Add(faces[i]);
                            }
                            stk.Push((locX + faces[i].nX) | ((locY + faces[i].nY) << 8) | ((locZ + faces[i].nZ) << 16));
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
        foreach (Face f in faces)
        {
            //Debug.Log(f.connectedFaces.Count + " connections");
        }
        Profiler.EndSample();


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
            Chunk next = World.getChunk(chunk.getCoordinates() + getNormal());
            if (next != null)
            {
                opposingFace = next.getFaceByNormal(-1 * getNormal());
                return opposingFace;
            }
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

}

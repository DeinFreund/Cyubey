using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class ChunkDrawer : MonoBehaviour {

    private const bool enableDebugColors = false;
    public int debugIndex = 0;

	// Use this for initialization
	void Start () {
	
	}

    HashSet<Chunk> chunksProcessed;
    Matrix4x4 toCamMatrix;

    

    private HashSet<Chunk> invisibleChunks = new HashSet<Chunk>();
    private HashSet<Coordinates> interestingChunks = null;
    private HashSet<Chunk.Face> startfaces;

    private Vector3 vecpos;
    private Position pos;

    private struct Args
    {
        public Chunk c; public Chunk.Face incomingFace; public float bottomLeftX; public float bottomLeftY; public float topRightX; public float topRightY;

        public Args(Chunk c, Chunk.Face incomingFace, float bottomLeftX, float bottomLeftY, float topRightX, float topRightY)
        {
            this.c = c;
            this.incomingFace = incomingFace;
            this.bottomLeftX = bottomLeftX;
            this.bottomLeftY = bottomLeftY;
            this.topRightX = topRightX;
            this.topRightY = topRightY;
        }
    }

    private void drawChunk(Chunk c, Chunk.Face incomingFace, float bottomLeftX, float bottomLeftY, float topRightX, float topRightY)
    {
        Vector3[] corners = new Vector3[8];
        Args arg;
        Stack<Args> stack = new Stack<Args>();
        stack.Push(new Args(c, incomingFace, bottomLeftX, bottomLeftY, topRightX, topRightY));
        int maxdep = 0;
        while (stack.Count > 0)
        {
            Profiler.BeginSample("args");

            arg = stack.Pop();
            c = arg.c;
            incomingFace = arg.incomingFace;
            bottomLeftX = arg.bottomLeftX;
            bottomLeftY = arg.bottomLeftY;
            topRightX = arg.topRightX;
            topRightY = arg.topRightY;

            //args = new Pair<Chunk, Chunk.Face>(c, null);
            Profiler.EndSample();
            if (c == null) continue;
            if (chunksProcessed.Contains(c))
            {
                continue;
            }
            Profiler.BeginSample("args2");
            chunksProcessed.Add(c);

            int cx = c.getXOffset() - pos.x;
            int cy = c.getYOffset() - pos.y;
            int cz = c.getZOffset() - pos.z;
            int s = Chunk.size;

            Profiler.EndSample();
            Profiler.BeginSample("evaluation");



            corners[0] = Camera.main.WorldToScreenPoint(new Vector3(cx + 0, cy + 0, cz + 0));
            corners[1] = Camera.main.WorldToScreenPoint(new Vector3(cx + 0, cy + 0, cz + s));
            corners[2] = Camera.main.WorldToScreenPoint(new Vector3(cx + 0, cy + s, cz + 0));
            corners[3] = Camera.main.WorldToScreenPoint(new Vector3(cx + 0, cy + s, cz + s));
            corners[4] = Camera.main.WorldToScreenPoint(new Vector3(cx + s, cy + 0, cz + 0));
            corners[5] = Camera.main.WorldToScreenPoint(new Vector3(cx + s, cy + 0, cz + s));
            corners[6] = Camera.main.WorldToScreenPoint(new Vector3(cx + s, cy + s, cz + 0));
            corners[7] = Camera.main.WorldToScreenPoint(new Vector3(cx + s, cy + s, cz + s));

            /*
            if (c.getX() == 0 && c.getY() == -1 && c.getZ() == 0)
            {
                for (int i = 0; i < corners.Length; i++)
                    GameObject.Find("Image (" + i + ")").GetComponent<RectTransform>().position = corners[i];
            }*/


            float minX = Camera.main.pixelWidth;
            float maxX = 0;
            float minY = Camera.main.pixelHeight;
            float maxY = 0;

            int behind = 0;

            foreach (Vector3 corner in corners)
            {
                if (corner.z <= 0)
                {
                    //Corner behind camera
                    behind++;
                    continue;
                }
                minX = System.Math.Min(minX, corner.x);
                minY = System.Math.Min(minY, corner.y);
                maxX = System.Math.Max(maxX, corner.x);
                maxY = System.Math.Max(maxY, corner.y);

            }



            if (c.getX() == 0 && c.getY() == -1 && c.getZ() == 0)
            {
                //Debug.Log(minX + "|" + minY + " - " + maxX + "|" + maxY + " / " + bottomLeftX + "|" + bottomLeftY + " - " + topRightX + "|" + topRightY + " => " + (minX >= topRightX || minY >= topRightY || maxX <= bottomLeftX || maxY <= bottomLeftY) + " inc: " + incomingFace);
            }

            minX = System.Math.Max(0, minX);
            minY = System.Math.Max(0, minY);
            maxX = System.Math.Min(Camera.main.pixelWidth, maxX);
            maxY = System.Math.Min(Camera.main.pixelHeight, maxY);

            if (c.getX() == 0 && c.getY() == -1 && c.getZ() == 0 && (minX >= topRightX || minY >= topRightY || maxX <= bottomLeftX || maxY <= bottomLeftY))
            {
                //Debug.Log(minX + "|" + minY + " - " + maxX + "|" + maxY + " / " + bottomLeftX + "|" + bottomLeftY + " - " + topRightX + "|" + topRightY + " => " + (minX >= topRightX || minY >= topRightY || maxX <= bottomLeftX || maxY <= bottomLeftY));
            }
            Profiler.EndSample();
            if (behind == 8)
            {
                //Chunk behind camera
                if (enableDebugColors) c.setDebugColor(Color.blue);
                continue;
            }
            if (minX >= topRightX || minY >= topRightY || maxX <= bottomLeftX || maxY <= bottomLeftY)
            {
                //Chunk outside FOV
                if (enableDebugColors) c.setDebugColor(Color.red);
                continue;
            }

            Profiler.BeginSample("recursion");

            if (c.getX() == 0 && c.getY() == -1 && c.getZ() == 0)
            {
                //Debug.Log("rendering");
            }
            /*
            if (enableDebugColors) c.setDebugColor(Color.green);
            if (maxdep++ == debugIndex)
            {
                GameObject.Find("Image (0)").GetComponent<RectTransform>().position = new Vector3(minX, maxY, 0);
                GameObject.Find("Image (1)").GetComponent<RectTransform>().position = new Vector3(maxX, minY, 0);
                GameObject.Find("Image (2)").GetComponent<RectTransform>().position = new Vector3(minX, minY, 0);
                GameObject.Find("Image (3)").GetComponent<RectTransform>().position = new Vector3(maxX, maxY, 0);
            }*/
            c.setRendered(true);
            invisibleChunks.Remove(c);

            ICollection<Chunk.Face> faces;
            if (incomingFace != null)
            {
                faces = incomingFace.getConnectedFaces();
            }
            else
            {
                faces = c.getFaces();
            }
            //Debug.Log("Drawing chunk " + c.getCoordinates() + " with " + faces.Count + " connections");

            Profiler.EndSample();
            Debug.DrawRay(c.getCenter() + new Vector3(1,1,1) - vecpos, (Vector3)incomingFace.getNormal() * 5, maxdep++ == 0 ? Color.green : Color.blue, 0.1f);
            //if (maxdep++ > 4) break;
            
            foreach (Chunk.Face face in faces)
            {
                if (Vector3.Dot((c.getCenter() - vecpos).normalized, face.getNormal()) < -0.01f && (c.getCenter() - vecpos).magnitude > Chunk.size * 3)
                {
                    continue;
                }
                if (face.getOpposingFace() == null)
                {
                    if (interestingChunks != null) interestingChunks.Add(c.getCoordinates() + face.getNormal());
                    continue;
                }
                Debug.DrawRay(new Vector3(), c.getCenter() - vecpos, Color.yellow, 0.1f);
                Debug.DrawRay(c.getCenter() - vecpos, (Vector3)face.getNormal()* 5, incomingFace == null ? Color.blue : Color.red, 0.1f);
                stack.Push(new Args(face.getOpposingFace().getChunk(), face.getOpposingFace(), minX, minY, maxX, maxY));
            }
        }
    }

	// Update is called once per frame
	void Update () {
        
        pos = MovementController.feetPosition.above();
        vecpos = -MovementController.worldParent.transform.position;
        //pos = new Position(0, 0, 0);
        if (pos.getChunk() != null)
        {

            if (World.getRequestCount() < 1 && Time.frameCount % 1 == 0)
            {
                //Debug.Log("Looking for interesting chunks");
                interestingChunks = new HashSet<Coordinates>();
            }else
            {
                //Debug.Log("not Looking for interesting chunks");
                interestingChunks = null;
            }
            Profiler.BeginSample("chunks");
            startfaces = new HashSet<Chunk.Face>();
            toCamMatrix = Camera.main.worldToCameraMatrix;
            invisibleChunks.Clear();
            //startchunks.Add(pos.getChunk());
            lock (World.getChunks())
            {
                foreach (Chunk c in World.getChunks().Values)
                {
                    invisibleChunks.Add(c);
                    if (enableDebugColors) c.setDebugColor(Color.yellow);
                    if (((Coordinates)c.getCenter()).distanceTo(pos) < 1.1 * Chunk.size)
                    {
                        Chunk c2 = c;
                        Chunk.Face best = c2.getFaces()[0];
                        foreach (Chunk.Face f in c2.getFaces())
                        {
                            if (Vector3.Dot(vecpos - c2.getCenter(), f.getNormal()) > Vector3.Dot(vecpos - c2.getCenter(), best.getNormal()))
                            {
                                best = f;
                            }
                        }
                        startfaces.Add(best);
                    }
                }
            }

            chunksProcessed = new HashSet<Chunk>();
            foreach (Chunk.Face f in startfaces)
            {
                invisibleChunks.Remove(f.getChunk());
                f.getChunk().setRendered(true);
                Profiler.BeginSample("drawchunks");
                drawChunk(f.getChunk(), f, 0, 0, Camera.main.pixelWidth, Camera.main.pixelHeight);
                Profiler.EndSample();
            }
            
            foreach (Chunk c in invisibleChunks)
            {
                c.setRendered(enableDebugColors);
            }
            Profiler.EndSample();
            if (interestingChunks != null)
            {
                float viewdist = World.getMaxViewDistance() * Chunk.size;
                float score, best = float.MaxValue;
                Vector3 toChunk;
                Vector3 view = Camera.main.transform.forward.normalized;
                Coordinates chunk = new Coordinates(0, 0, 0);
                foreach (Coordinates c in interestingChunks)
                {
                    toChunk = (Chunk.size * c) - (Vector3)pos;
                    score = Vector3.Dot(toChunk, view) + Vector3.Cross(toChunk, view).magnitude * 3;
                    if (score < best && toChunk.magnitude < viewdist)
                    {
                        best = score;
                        chunk = c;
                    }
                }
                /*
                if (chunk == null)
                {
                    foreach (Chunk c in World.getChunks())
                    { 
                        foreach (Chunk.Face f in c.getFaces())
                        {
                            if (World.getChunk(c.getCoordinates() + f.getNormal()) == null && c.getCenter().distanceTo(pos) < best)
                            {
                                best = c.getCenter().distanceTo(pos);
                                chunk = c.getCoordinates() + f.getNormal();
                            }
                        }
                    }
                }*/
                    World.requestChunkLoad(chunk);
            }
            //Debug.Log("Drawing " + (World.getChunks().Count - invisibleChunks.Count) + " of " + World.getChunks().Count + " chunks");
        }
	}

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }


        public override int GetHashCode()
        {
            return (First != null ? First.GetHashCode() : 429084) ^ (Second != null ? Second.GetHashCode() : 245451337);
        }

        public override bool Equals(System.Object other)
        {
            return GetHashCode() == other.GetHashCode();
        }

    };
}

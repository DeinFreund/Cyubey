using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public class ChunkManager
{

    private int size;
    private string path;

    private static ChunkManager singleton = new ChunkManager(16, "saves");

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

    public static void RequestChunkSave(Coordinates pos, byte[] data)
    {
        //Can be later optimized to batch saves of nearby chunks
        singleton.SaveChunk(pos, data);
    }

    private void SaveChunk(Coordinates position, byte[] chunkdata)
    {

        string xs = Math.Floor((double)position.x / size).ToString();
        string ys = Math.Floor((double)position.y / size).ToString();
        string zs = Math.Floor((double)position.z / size).ToString();

        int pos = ((position.x % size + size) % size) + (size * ((position.x % size + size) % size)) + (size * size * ((position.x % size + size) % size));
        byte[] prev;
        byte[] result;
        int space = 0;
        int oldLength;
        int newLength = chunkdata.Length;

        if (!File.Exists(path + "/" + xs + ys + zs))
        {
            result = new byte[(size * size * size * 4) + newLength];

            space = 4 * pos;
        }
        else
        {
            prev = File.ReadAllBytes(path + "/" + xs + ys + zs);

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

        File.WriteAllBytes(path + "/" + xs + ys + zs, result);
    }

    public static void RequestChunkLoad(Chunk chunk)
    {
        //Can be optimized to load nearby chunks as well to reduce FS load
        MainThread.runSoon(() => chunk.deserialize(singleton.LoadChunk(chunk.getX(), chunk.getY(), chunk.getZ())));
    }

    private byte[] LoadChunk(int x, int y, int z)
    {
        //find file
        string xs = Math.Floor((double)x / (double)size).ToString();
        string ys = Math.Floor((double)y / (double)size).ToString();
        string zs = Math.Floor((double)z / (double)size).ToString();
        int chunkLength;
        int pos = ((x % size + size) % size) + (size * ((y % size + size) % size)) + (size * size * ((z % size + size) % size));

        if (!File.Exists(path + "/" + xs + ys + zs))
        {
            //Debug.LogError("No region file " + xs + " " + ys + " " + zs + " for this chunk");
            //Expected behaviour..
            return new byte[0];
        } //Region does not exist

        byte[] bytes = File.ReadAllBytes(path + "/" + xs + ys + zs);

        int space = 0;
        for (int i = 0; i < pos; i++)
        {
            space += BitConverter.ToInt32(bytes, space) + 4;
        }

        chunkLength = BitConverter.ToInt32(bytes, space);
        if (chunkLength == 0)
        {
            //Debug.LogError("No chunk at position " + x.ToString() + " " + y.ToString() + " " + z.ToString());
            //Expected behaviour..
            return new byte[0];
        } // no chunk saved here

        byte[] chunk = new byte[chunkLength];
        for (int i = 0; i < chunkLength; i++)
        {
            chunk[i] = bytes[space + 4 + i];
        }

        return chunk;
    }

    //should probably use unit tests for this..
    public static void Test()
    {
        //magic test
        ChunkManager test1 = new ChunkManager(2, "chunks/for/me");
        test1.SaveChunk(new Coordinates(0, -1, 0), new byte[] { 21, 53 });
        test1.LoadChunk(0, -1, 0);

        //Chunk saving and loading test
        ChunkManager test = new ChunkManager(16, "Chunks");
        int err = 0;
        int cnt = 300;
        Vector3[] co = new Vector3[cnt];
        byte[][] chunks = new byte[cnt][];
        byte[][] saved = new byte[cnt][];

        for (int i = 0; i < cnt; i++)
        {
            co[i] = UnityEngine.Random.insideUnitSphere * 50;
            int bytes = (int)Mathf.Ceil(UnityEngine.Random.value * cnt);
            chunks[i] = new byte[bytes];
            for (int j = 0; j < bytes; j++)
            {
                chunks[i][j] = (byte)(UnityEngine.Random.value * 255);
            }
        }
        for (int i = 0; i < cnt; i++)
        {
            test.SaveChunk(new Coordinates((int)co[i].x, (int)co[i].y, (int)co[i].z), chunks[i]);
        }

        for (int i = 0; i < cnt; i++)
        {
            saved[i] = test.LoadChunk((int)co[i].x, (int)co[i].y, (int)co[i].z);
        }

        for (int i = 0; i < cnt; i++)
        {
            if (!Enumerable.SequenceEqual(chunks[i], saved[i]))
            {

                string a = "";
                for (int j = 0; j < chunks[i].Length; j++)
                {
                    a += chunks[i][j].ToString() + " ";
                }
                a += " expected, but got ";
                for (int j = 0; j < chunks[i].Length; j++)
                {
                    a += saved[i][j].ToString() + " ";
                }
                Debug.LogError(a);
                err++;
            }
        }
        Debug.Log("Load errors:" + err);
    }
}

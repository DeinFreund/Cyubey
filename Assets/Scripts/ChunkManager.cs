using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ChunkManager{

    private int size;

    public ChunkManager(int size) { this.size = size; }

    public void SaveChunk(int x, int y, int z, byte[] chunks) {
        //find file

        string xs = Math.Floor((double)x / (double)size).ToString();
        string ys = Math.Floor((double)y / (double)size).ToString();
        string zs = Math.Floor((double)z / (double)size).ToString();

        int pos = ((x % size + size) % size) + (size * ((y % size + size) % size)) + (size * size * ((z % size + size) % size));
        byte[] prev;
        byte[] result;
        int space = 0;
        int oldLength;
        int newLength = chunks.Length;

        if (!File.Exists(xs + ys + zs)) {
            result = new byte[(size * size * size * 4) + newLength];

            space = 4 * pos;
        } else {
            prev = File.ReadAllBytes(xs + ys + zs);

            for (int i = 0; i < pos; i++) {
                space += 4 + BitConverter.ToInt32(prev, space);
            }

            oldLength = BitConverter.ToInt32(prev, space);
            result = new byte[prev.Length + newLength - oldLength];

            for (int i = 0; i < space; i++) {
                result[i] = prev[i];
            }

            for (int i = space + 4 + newLength; i < result.Length; i++) {
                result[i] = prev[i - newLength + oldLength];
            }
        }

        Debug.Log("Insert " + chunks.Length + " Bytes at Byte " + space);

        for (int i = 0; i < 4; i++) {
            result[space + i] = BitConverter.GetBytes(newLength)[i];
        }

        for (int i = 0; i < chunks.Length; i++) {
            result[space + i + 4] = chunks[i];
        }
        
        File.WriteAllBytes(xs + ys + zs, result);
    }

    public byte[] LoadChunk(int x, int y, int z) {
        //find file
        string xs = (x / size).ToString();
        string ys = (x / size).ToString();
        string zs = (x / size).ToString();
        int pos = x % size + (size * (y % size)) + (size * size * (z % size));

        if (!File.Exists(xs + ys + zs)) return null; //Region does not exist

        byte[] bytes = File.ReadAllBytes(xs + ys + zs);

        int space = 0;
        for (int i = 0; i < pos; i++) {
            space += BitConverter.ToInt32(bytes, space) + 4;
        }

        pos = BitConverter.ToInt32(bytes, space);
        if (pos == 0) return null; // no chunk saved here

        byte[] chunk = new byte[pos];
        for(int i = 0; i < pos; i++) {
            chunk[i] = bytes[space + i];
        }

        return chunk;
    }
}

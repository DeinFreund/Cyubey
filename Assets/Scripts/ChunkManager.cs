using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ChunkManager{

    private int size;
    private string path;

    public ChunkManager(int size, string path) {
        this.size = size;
        this.path = path;

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }

    public void SaveChunk(int x, int y, int z, byte[] chunks) {

        string xs = Math.Floor((double)x / (double)size).ToString();
        string ys = Math.Floor((double)y / (double)size).ToString();
        string zs = Math.Floor((double)z / (double)size).ToString();

        int pos = ((x % size + size) % size) + (size * ((y % size + size) % size)) + (size * size * ((z % size + size) % size));
        byte[] prev;
        byte[] result;
        int space = 0;
        int oldLength;
        int newLength = chunks.Length;

        if (!File.Exists(path + "/" + xs + ys + zs)) {
            result = new byte[(size * size * size * 4) + newLength];

            space = 4 * pos;
        } else {
            prev = File.ReadAllBytes(path + "/" + xs + ys + zs);

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
        
        File.WriteAllBytes(path + "/" + xs + ys + zs, result);
    }

    public byte[] LoadChunk(int x, int y, int z) {
        //find file
        string xs = Math.Floor((double)x / (double)size).ToString();
        string ys = Math.Floor((double)y / (double)size).ToString();
        string zs = Math.Floor((double)z / (double)size).ToString();
        int chunkLenght;
        int pos = ((x % size + size) % size) + (size * ((y % size + size) % size)) + (size * size * ((z % size + size) % size));

        if (!File.Exists(path + "/" + xs + ys + zs)) {
            Debug.LogError("No region file " + xs + " " + ys + " " + zs + " for this chunk");
            return null;
        } //Region does not exist

        byte[] bytes = File.ReadAllBytes(path + "/" + xs + ys + zs);

        int space = 0;
        for (int i = 0; i < pos; i++) {
            space += BitConverter.ToInt32(bytes, space) + 4;
        }

        chunkLenght = BitConverter.ToInt32(bytes, space);
        if (chunkLenght == 0) {
            Debug.LogError("No chunk at position " + x.ToString() + " " + y.ToString() + " " + z.ToString());
            return null;
        } // no chunk saved here
        
        byte[] chunk = new byte[chunkLenght];
        for(int i = 0; i < chunkLenght; i++) {
            chunk[i] = bytes[space + 4 + i];
        }

        return chunk;
    }
}

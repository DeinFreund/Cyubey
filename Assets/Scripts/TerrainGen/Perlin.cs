using UnityEngine;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;

public class Perlin : IGenerator {

    private int seed;
    private float[] weights = {0.01f,0.05f,0.1f,0.5f};
    private int sizeSkip = 2; //small details to skip
    private float[] randoms = new float[1000000];

    private int next2(int num)
    {
        int res = 1;
        while (res < num) res <<= 1;
        return res;
    }

    public Perlin(int seed, float[] weights)
    {
        System.Random rnd = new System.Random(seed);
        for (int i = 0; i < randoms.Length; i++)
        {
            randoms[i] = (float)rnd.NextDouble();
        }
        if (weights != null)
        {
            this.weights = weights;
        }
        this.seed = seed;
        
        if (false)
        {
            int s = 512;
            for (int z = 0; z < 1; z++)
            {
                StringBuilder img = new StringBuilder();
                img.Append("P2\n" + s + " " + s + "\n128\n");

                for (int x = 0; x < s; x++)
                {
                    for (int y = 0; y < s; y++)
                    {
                        img.Append((int)(Mathf.Pow(getValue(x, y, z), 2f) * 128));
                        img.Append(' ');
                    }
                    img.Append('\n');
                }
                FileIO.write("img" + z + ".pgm", img.ToString());
            }
        }
    }

    private int nextPrime(int start)
    {
        while (true)
        {
            start++;
            bool prime = true;
            for (int i = 2; i * i <= start; i++)
            {
                if (start % i == 0)
                {
                    prime = false;
                    break;
                }
            }
            if (prime)
            {
                return start;
            }
        }
    }
    
    private float random(long x, long y, long z)
    {
        return randoms[((((x  + y * 30011 + z * 1000003 + seed * 997) % 2000000011 + 2000000011) % 2000000011)) % randoms.Length];
    }

    private System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();


    private float random(long seed)
    {
        //System.Random rnd = new System.Random((int)seed);
        //return murmur.Hash(new byte[] { (byte)seed, (byte)(seed >> 8), (byte)(seed >> 16), (byte)(seed >> 24) }) / (float)uint.MaxValue;
        //return (float)rnd.NextDouble();
        //return md5.ComputeHash(new byte[] { (byte)seed, (byte)(seed >> 8), (byte)(seed >> 16), (byte)(seed >> 24) })[seed % 4] / 256f;
        return ((((2000000011L * seed + 1013904223L) ) % 65536) / (float)(65536));
    }
    
    private float interp(float v0, float v1, float fac)
    {
        return v0 * (1 - fac) + v1 * fac;
    }

    public float getValue(Coordinates coords)
    {
        return getValue(coords.getX(), coords.getY(), coords.getZ());
    }

    public float getValue(int x, int y, int z)
    {
        float totweights = 0;
        float result = 0;
        float fx, fy, fz;
        fx = x;
        fy = y;
        fz = z;
        x >>= sizeSkip;
        y >>= sizeSkip;
        z >>= sizeSkip;
        fx /= (1 << sizeSkip);
        fy /= (1 << sizeSkip);
        fz /= (1 << sizeSkip);
        foreach (float weight in weights)
        {
            totweights += weight;
            float ex0 = interp(random(x, y, z), random(x + 1, y, z), fx - x);
            float ex1 = interp(random(x, y, z + 1), random(x + 1, y, z + 1), fx - x);
            float ex2 = interp(random(x, y + 1, z), random(x + 1, y + 1, z), fx - x);
            float ex3 = interp(random(x, y + 1, z + 1), random(x + 1, y + 1, z + 1), fx - x);
            float ez0 = interp(ex0, ex1, fz - z);
            float ez1 = interp(ex2, ex3, fz - z);
            result += weight * interp(ez0, ez1, fy - y);
            x >>= 1;
            y >>= 1;
            z >>= 1;
            fx /= 2;
            fy /= 2;
            fz /= 2;
        }
        return result / totweights;
    }
    public float[,,] getValues(Coordinates start, Coordinates end)
    {
        float[,,] retval = new float[end.x - start.x, end.y - start.y, end.z - start.z];
        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                for (int z = start.z; z < end.z; z++)
                {
                    float totweights = 0;
                    float result = 0;
                    float fx, fy, fz;
                    int xx = x >> sizeSkip;
                    int yy = y >> sizeSkip;
                    int zz = z >> sizeSkip;
                    fx = xx;
                    fy = yy;
                    fz = zz;
                    foreach (float weight in weights)
                    {
                        totweights += weight;
                        float ex0 = interp(random(xx, yy, zz), random(xx + 1, yy, zz), fx - xx);
                        float ex1 = interp(random(xx, yy, zz + 1), random(xx + 1, yy, zz + 1), fx - xx);
                        float ex2 = interp(random(xx, yy + 1, zz), random(xx + 1, yy + 1, zz), fx - xx);
                        float ex3 = interp(random(xx, yy + 1, zz + 1), random(xx + 1, yy + 1, zz + 1), fx - xx);
                        float ez0 = interp(ex0, ex1, fz - zz);
                        float ez1 = interp(ex2, ex3, fz - zz);
                        result += weight * interp(ez0, ez1, fy - yy);
                        xx >>= 1;
                        yy >>= 1;
                        zz >>= 1;
                        fx /= 2;
                        fy /= 2;
                        fz /= 2;
                    }
                    retval[x - start.x, y - start.y, z - start.z] = result / totweights;
                }
            }
        }
        return retval;
    }
}

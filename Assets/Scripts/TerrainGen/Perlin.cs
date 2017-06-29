using UnityEngine;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

public class Perlin : IGenerator {

    private readonly int seed;
    private readonly float[] weights = { 0.01f, 0.05f, 0.1f, 0.5f };
    private readonly int sizeSkip = 2; //small details to skip
    private readonly float[] randoms = new float[1000000];
    private readonly float[] offsetsX;
    private readonly float[] offsetsY;
    private readonly float[] offsetsZ;
    private readonly float totweights = 0;

    private int next2(int num)
    {
        int res = 1;
        while (res < num) res <<= 1;
        return res;
    }

    public Perlin(int seed, float[] weights, int weightOffset)
    {
        System.Random rnd = new System.Random(seed);
        for (int i = 0; i < randoms.Length; i++)
        {
            randoms[i] = (float)rnd.NextDouble();
        }
        if (weights != null)
        {
            this.sizeSkip = weightOffset;
            this.weights = weights;
        }
        this.totweights = weights.Sum();
        offsetsX = new float[weights.Length];
        offsetsY = new float[weights.Length];
        offsetsZ = new float[weights.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            offsetsX[i] = (float)rnd.NextDouble() * 1000;
            offsetsY[i] = (float)rnd.NextDouble() * 1000;
            offsetsZ[i] = (float)rnd.NextDouble() * 1000;
        }
        this.seed = seed;
        
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
        return randoms[((((x + y * 30011 + z * 1000003 + seed * 997) % 2000000011 + 2000000011) % 2000000011)) % randoms.Length];
    }

    private System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();


    private float random(long seed)
    {
        //System.Random rnd = new System.Random((int)seed);
        //return murmur.Hash(new byte[] { (byte)seed, (byte)(seed >> 8), (byte)(seed >> 16), (byte)(seed >> 24) }) / (float)uint.MaxValue;
        //return (float)rnd.NextDouble();
        //return md5.ComputeHash(new byte[] { (byte)seed, (byte)(seed >> 8), (byte)(seed >> 16), (byte)(seed >> 24) })[seed % 4] / 256f;
        return ((((2000000011L * seed + 1013904223L)) % 65536) / (float)(65536));
    }

    private float interp(float v0, float v1, float fac)
    {
        return v0 * (1 - fac) + v1 * fac;
    }
    

    public void fillArray(Coordinates coords, float[,,] array)
    {
        /*getValues(coords, coords + new Coordinates(array.GetLength(0), array.GetLength(1), array.GetLength(2)), array);
        return;*/
        float fx, fy, fz;

        int x, y, z, i;
        float result;
        for (x = 0; x < array.GetLength(0); x++)
        {
            for (y = 0; y < array.GetLength(1); y++)
            {
                for (z = 0; z < array.GetLength(2); z++)
                {
                    result = 0;
                    fx = x + coords.x;
                    fy = y + coords.y;
                    fz = z + coords.z;
                    fx /= (1 << sizeSkip);
                    fy /= (1 << sizeSkip);
                    fz /= (1 << sizeSkip);
                    for (i = 0; i < weights.Length; ++i)
                    {
                        result += weights[i] * (float)SimplexNoise.noise(fx - offsetsX[i], fy - offsetsY[i], fz - offsetsZ[i]);
                        fx /= 2;
                        fy /= 2;
                        fz /= 2;
                    }
                    array[x, y, z] = result / totweights;
                }
            }
        }
    }
    public void getValues(Coordinates start, Coordinates end, float[,,] retval)
    {
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
    }
}

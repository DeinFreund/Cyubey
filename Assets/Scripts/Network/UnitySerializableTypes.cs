using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public struct Vector3Serializer
{
    public float x;
    public float y;
    public float z;

    public Vector3Serializer(Vector3 v3)
    {
        x = v3.x;
        y = v3.y;
        z = v3.z;
    }

    public static implicit operator Vector3(Vector3Serializer coords)
    {
        return coords.V3;
    }

    public static implicit operator Vector3Serializer(Vector3 coords)
    {
        return new Vector3Serializer(coords);
    }

    public Vector3 V3
    { get { return new Vector3(x, y, z); } }
}

[Serializable]
public struct QuaternionSerializer
{
    public float x;
    public float y;
    public float z;
    public float w;

    public QuaternionSerializer(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public static implicit operator Quaternion(QuaternionSerializer coords)
    {
        return coords.Q;
    }

    public static implicit operator QuaternionSerializer(Quaternion coords)
    {
        return new QuaternionSerializer(coords);
    }

    public Quaternion Q
    { get { return new Quaternion(x, y, z, w); } }
}
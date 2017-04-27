using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public struct Coordinates
{
    public readonly int x, y, z;
    
    public Coordinates(int locX, int locY, int locZ)
    {
        this.x = locX;
        this.y = locY;
        this.z = locZ;
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

    public float distanceTo(Coordinates other)
    {
        return Mathf.Sqrt((getX() - other.getX())*(getX() - other.getX()) + 
            (getY() - other.getY()) * (getY() - other.getY()) + 
            (getZ() - other.getZ()) * (getZ() - other.getZ()));
    }

    
    public override int GetHashCode()
    {
        return (int)((getX() + (long)getY() * 30011 + (long)getZ() * 7368787) % 1000000007);
    }

    public override bool Equals(System.Object other)
    {
        return other is Coordinates && (Equals((Coordinates)other));
    }

    public bool Equals(Coordinates other)
    {
        return other.getX() == getX() && other.getY() == getY() && other.getZ() == getZ();
    }

    public override string ToString()
    {
        return getX() + "|" + getY() + "|" + getZ();
    }

    public Coordinates div_floor(int size)
    {
        return new Coordinates(Position.div_floor(x, size), Position.div_floor(y, size), Position.div_floor(z, size));
    } 

    public static Coordinates operator +(Coordinates c1, Coordinates c2)
    {
        return new Coordinates(c1.getX() + c2.getX(), c1.getY() + c2.getY(), c1.getZ() + c2.getZ());
    }

    public static Coordinates operator -(Coordinates c1, Coordinates c2)
    {
        return c1 + -1*c2;
    }

    public static Coordinates operator *(int c1, Coordinates c2)
    {
        return new Coordinates(c1 * c2.getX(), c1 * c2.getY(), c1 * c2.getZ());
    }

    public static Coordinates operator *(Coordinates c2, int c1)
    {
        return new Coordinates(c1 * c2.getX(), c1 * c2.getY(), c1 * c2.getZ());
    }

    public static implicit operator Vector3(Coordinates coords) 
    {
        return new Vector3(coords.getX(), coords.getY(), coords.getZ()); 
    }

    public static implicit operator Position(Coordinates coords)
    {
        return new Position(coords.getX(), coords.getY(), coords.getZ());
    }

}


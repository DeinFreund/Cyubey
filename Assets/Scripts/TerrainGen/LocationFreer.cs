
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationFreer : IGenerator
{
    protected IGenerator child;
    protected List<Coordinates> locations = new List<Coordinates>();
    protected List<int> innerRadii = new List<int>();
    protected List<int> outerRadii = new List<int>();

    public LocationFreer(IGenerator child)
    {
        this.child = child;
    }

    public void freeLocation(Coordinates location, int radius)
    {
        freeLocation(location, radius, radius * 2);
    }


    public void freeLocation(Coordinates location, int innerRadius, int outerRadius)
    {
        locations.Add(location);
        innerRadii.Add(innerRadius);
        outerRadii.Add(outerRadius);
    }

    public void fillArray(Coordinates coords, float[,,] array)
    {
        child.fillArray(coords, array);
        int x, y, z;
        for (int i = 0; i < locations.Count; i++)
        {
            Coordinates start = locations[i] - new Coordinates(1, 1, 1) * outerRadii[i] - coords;
            Coordinates end = locations[i] + new Coordinates(1, 1, 1) * outerRadii[i] - coords;
            int startx = Math.Max(start.x, 0);
            int starty = Math.Max(start.y, 0);
            int startz = Math.Max(start.z, 0);
            int endx = Math.Min(end.x, array.GetLength(0));
            int endy = Math.Min(end.y, array.GetLength(1));
            int endz = Math.Min(end.z, array.GetLength(2));
            float diff = outerRadii[i] - innerRadii[i];
            for (x = startx; x < endx; x++)
            {
                for (y = starty; y < endy; y++)
                {
                    for (z = startz; z < endz; z++)
                    {
                        array[x, y, z] *= Math.Min(Math.Max((Vector3.ProjectOnPlane(new Vector3(coords.x + x, coords.y + y, coords.z + z) - locations[i], Vector3.zero).magnitude - innerRadii[i]) / (diff), 0f), 1f);
                    }
                }
            }
        }
    }
}


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

    public float getValue(Coordinates coords)
    {

        float spawnchance = 1;
        for (int i = 0; i < locations.Count; i++)
        {
            spawnchance *= Math.Min(Math.Max((Vector3.ProjectOnPlane((Vector3)coords - locations[i], Vector3.zero).magnitude - innerRadii[i]) / (outerRadii[i] - innerRadii[i]), 0f), 1f);
        }
        return child.getValue(coords) * spawnchance;
    }
}

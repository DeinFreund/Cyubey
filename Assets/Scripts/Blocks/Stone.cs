using UnityEngine;
using System.Collections;

public class Stone : Block
{

    public static new int ID = 1;

    public Stone() : base()
    {

    }

    public override int getMeshID(Coordinates coords)
    {
        return 1;
    }

    public override int getID()
    {
        return ID;
    }
}

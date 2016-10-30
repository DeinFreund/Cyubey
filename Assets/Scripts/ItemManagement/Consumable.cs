using UnityEngine;
using System.Collections;

public class Consumable : Item
{
    public Consumable(string name, int id, string desc) : base(name, id, desc)
    {
        itemType = ItemType.Consumable;
        itemMaxCount = 50;
    }

    public void eat()
    {
        Debug.Log("Yum");
    }
}

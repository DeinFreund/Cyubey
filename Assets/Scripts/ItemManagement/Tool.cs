using UnityEngine;
using System.Collections;

public class Tool : Item
{
    public int toolDurability;
    public int toolHealth;

    public Tool(string name, int id, string desc, int durability) : base(name, id, desc)
    {
        itemType = ItemType.Tool;
        itemMaxCount = 1;
        toolDurability = durability;
        toolHealth = durability;
    }

    public void fullRepair()
    {
        toolHealth = toolDurability;
    }


}

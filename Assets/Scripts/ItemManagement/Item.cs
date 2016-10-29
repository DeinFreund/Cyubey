using UnityEngine;
using System.Collections;

[System.Serializable]
public class Item
{
    public string itemName;
    public int itemID;
    public string itemDesc;
    public Texture2D itemIcon;
    public ItemType itemType;

    public enum ItemType
    {
        Block,
        Tool
    }

    public Item() { }

    public Item(string name, int id, string desc)
    {
        itemName = name;
        itemID = id;
        itemDesc = desc;
        itemIcon = Resources.Load<Texture2D>("item_icons/" + id);
    }
}

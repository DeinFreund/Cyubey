using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class Item
{
    public int itemID;
    public string itemName;
    public string itemDesc;
    public Sprite itemIcon;

    public Item() { }

    public Item(int id, string name, string desc)
    {
        itemName = name;
        itemID = id;
        itemDesc = desc;
        itemIcon = Resources.Load<Sprite>("items/" + itemName);
    }

    public Sprite getIcon()
    {
        return itemIcon;
    }

    public int getID()
    {
        return itemID;
    }
}

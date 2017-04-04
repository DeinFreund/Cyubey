using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour {

    public Item[] items;
    public int counter;

    public ItemDatabase(int size)
    {
        items = new Item[size];
        counter = 0;
    }

    void Start()
    {
        items = new Item[256];
        counter = 0;

        registerItem("Wrench", "A tool");
        registerItem("Potato", "A Potato");
    }
    
    public void registerItem(string name, string desc)
    {
        items[counter] = new Item(counter, name, desc);
        counter++;
    }

    public Item ItemFromID(int id)
    {
        return items[id];
    }
}

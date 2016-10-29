using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {
    int space, width; //# of slots, maximum # of horizontal slots
    public List<Item> inventory = new List<Item>(); //items in inventory
    public List<Item> slots = new List<Item>();
    private ItemDatabase database; 

    void Start ()
    {
        for (int i = 0; i < space; i++)
        {
            slots.Add(new Item());
        }
        database = GameObject.FindGameObjectWithTag("Database").GetComponent<ItemDatabase>();
        inventory.Add(database.items[0]);
        inventory.Add(database.items[1]);
        inventory.Add(database.items[2]);
    }

    void OnGUI()
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            GUI.Label(new Rect(10, i * 20, 200, 50), inventory[i].itemName);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour {
    public List<Item> items = new List<Item>();

    void Start()
    {
        items.Add(new Tool("Wrench", 0, "A typical Wrench",100));
        items.Add(new Tool("Screwdriver", 1, "Fits for most screws", 150));
        items.Add(new Consumable("Potato", 2, "Why not"));
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public Item ItemFromID(int id)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if(items[i].itemID == id)
            {
                return items[i];
            }
        }
        return null;
    }
}

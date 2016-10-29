using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour {
    public List<Item> items = new List<Item>();

    void Start()
    {
        items.Add(new Tool("Wrench", 0, "A typical Wrench",100));
        items.Add(new Tool("Screwdriver", 1, "Fits for most screws", 150));
        items.Add(new Tool("Cake", 2, "Why not", 1));
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}

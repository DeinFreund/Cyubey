using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {
    
    private ItemDatabase database;
    public int width;
    public int height;
    public int barSize;
    public Slot[] slots;
    private Image draging;
    private bool pick;
    private int buffer;


    void Awake ()
    {
        draging = Instantiate<Image>(Resources.Load<Image>("items/dummy"));
        database = FindObjectOfType<ItemDatabase>();
        width = 10;
        height = 2;
        barSize = 8;
        slots = new Slot[width * height + barSize];
        for(int i = 0; i < width * height + barSize; i++)
        {
            slots[i] = new Slot();
        }
        AddSlot(0, 42);
        AddSlot(1, 24);
    }

    public void itemMove(int id)
    {
        print(id.ToString());
        if (!pick && getSlot(id).getItem() != null)
        {
            pick = true;
            buffer = id;
            draging = getSlot(id).getSprite();
            //draging.transform.SetSiblingIndex(0);
        }
        else if (pick)
        {
            pick = false;
            swapSlot(buffer, id);
        }
    }

    public Image getDraging()
    {
        return draging;
    }

    public bool getPick()
    {
        return pick;
    }

    public int getWidth()
    {
        return width;
    }

    public int getHeight()
    {
        return height;
    }

    public int getBarSize()
    {
        return barSize;
    }

    public Slot getSlot(int pos)
    {
        return slots[pos];
    }

    public void swapSlot(int from, int to)
    {
        Slot buff = slots[to];
        slots[to] = slots[from];
        slots[from] = buff;
    }

    public bool AddSlot(int id, int amount)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].getItem() != null)
            {
                if (slots[i].getItem().getID() == id)
                {
                    slots[i].addCount(amount);
                    return true;
                }
            }
            else
            {
                slots[i].setItem(database.ItemFromID(id));
                slots[i].setCount(amount);
                slots[i].getSprite().sprite = database.ItemFromID(id).getIcon();
                return true;
            }
        }
        return false;
    }

    /*
    public int RemoveItem(int id, int amount) //returns number of missing items
    {
        int count = amount;
        for (int i = inventory.Count-1; i >= 0; i--)
        {
            if(inventory[i].itemName != null && inventory[i].itemName == database.items[id].itemName)
            {
                if(counts[i] >= count)
                {
                    counts[i] -= count;
                    if(counts[i] == 0)
                    {
                        inventory[i] = new Item();
                    }
                    return 0;
                }
                else
                {
                    count -= counts[i];
                    counts[i] = 0;
                    inventory[i] = new Item();
                }
            }
        }
        return count;
    }

    public bool InInventory(int id, int amount)
    {
        int count = amount;
        for (int i = 0; i < inventory.Count; i++)
        {
            if(inventory[i].itemID == id)
            {
                if(counts[i] >= count)
                {
                    return true;
                }
                else
                {
                    count -= counts[i];
                }
            }
        }
        return false;
    } 
    */
}
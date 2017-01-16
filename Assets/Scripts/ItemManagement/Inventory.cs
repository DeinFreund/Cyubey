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
    public Image draging;
    public bool pickup;
    public int buffer;
    private GameObject[] canvas;

    void Awake ()
    {
        canvas = GameObject.FindGameObjectsWithTag("Item");
        draging = Instantiate<Image>(Resources.Load<Image>("items/dummy"));
        database = FindObjectOfType<ItemDatabase>();
        width = 10;
        height = 4;
        barSize = 8;
        slots = new Slot[width * height + barSize];
        for(int i = 0; i < width * height + barSize; i++)
        {
            slots[i] = new Slot();
        }
        AddSlot(0, 88);
        AddSlot(1, 69);
    }

    public bool AddSlot(int id, int amount)
    {
        for (int i = 0; i < slots.Length; i++)
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
                slots[i].getText().GetComponent<Text>().text = amount.ToString();
                return true;
            }
        }
        return false;
    }

    public void itemMove(int id)
    {
        print(id.ToString());
        if (!pickup && getSlot(id).getItem() != null)
        {
            pickup = true;
            buffer = id;
            draging = getSlot(id).getSprite();
            if(id < width * height)
            {
                canvas[0].transform.SetAsFirstSibling();
            }
            else
            {
                canvas[1].transform.SetAsFirstSibling();
            }
        }
        else if (pickup)
        {
            pickup = false;
            swapSlot(buffer, id);
            draging = null;
        }
    }

    public int getBuffer()
    {
        return buffer;
    }

    public Image getDraging()
    {
        return draging;
    }

    public bool getPick()
    {
        return pickup;
    }

    public void setPick(bool val)
    {
        pickup = val;
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
}
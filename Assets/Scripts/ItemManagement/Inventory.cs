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
    private GameObject dragedSlot;
    public Image draging;
    public bool pickup;
    public int buffer;
    private GameObject[] canvas;
    public bool locked;
    private bool shift;

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
        AddItem(0, 188);
        AddItem(1, 18);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            shift = true;
        }
        else
        {
            shift = false;
        }
    }

    public int AddItem(int id, int amount)
    {
        while (locked) ;
        locked = true;
        int count = amount;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].getItem() == null)
            {
                if(count > 99)
                {
                    slots[i] = new Slot(database.ItemFromID(id), 99);
                    count -= 99;
                }
                else
                {
                    slots[i] = new Slot(database.ItemFromID(id), count);
                    locked = false;
                    return 0;
                }
            }
            else
            {
                if(slots[i].getItem().getID() == id)
                {
                    if(slots[i].getCount() + count > 99)
                    {
                        slots[i] = new Slot(database.ItemFromID(id), 99);
                        count -= (99 - slots[i].getCount());
                    }
                    else
                    {
                        slots[i].addCount(count);
                        locked = false;
                        return 0;
                    }
                }
            }
        }
        locked = false;
        return count;
    }

    public int RemoveItem(int id, int amount)
    {
        while (locked) ;
        locked = true;
        int count = amount;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].getItem() != null)
            {
                if(slots[i].getItem().getID() == id)
                {
                    if(slots[i].getCount() < count)
                    {
                        count -= slots[i].getCount();
                        slots[i] = new Slot();
                    }
                    else
                    {
                        slots[i].removeCount(count);
                        return 0;
                    }
                }
            }
        }
        locked = false;
        return count;
    }

    public int CountItem(int id)
    {
        int counter = 0;
        for(int i = 0; i < width*height + barSize; i++)
        {
            if(slots[i].getItem() == database.ItemFromID(id))
            {
                counter += slots[i].getCount();
            }
        }
        return counter;
    }

    public void itemMove(int id)
    {
        print(id.ToString());
        if (!pickup && getSlot(id).getItem() != null)
        {
            if (shift)
            {
                if(id < width * height)
                {
                    for(int i = 0; i < barSize; i++)
                    {
                        if(slots[width * height + i].getItem() == null)
                        {
                            swapSlot(id, i + width * height);
                            return;
                        }
                    }
                } else
                {
                    for (int i = 0; i < width*height; i++)
                    {
                        if (slots[i].getItem() == null)
                        {
                            swapSlot(id, i);
                            return;
                        }
                    }
                }
            }
            pickup = true;
            buffer = id;
            draging = getSlot(id).getSprite();
            draging.transform.SetAsLastSibling();
            if(id < width * height)
            {
                canvas[0].transform.SetAsLastSibling();
            }
            else
            {
                canvas[1].transform.SetAsLastSibling();
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
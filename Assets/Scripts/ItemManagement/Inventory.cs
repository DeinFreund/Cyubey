using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {
    int height, width; //# of horizontal slots, vertical slots
    int margin, boxSize; //margin between slots, size of a slot
    public List<Item> inventory = new List<Item>();
    public List<int> counts = new List<int>(); //occurrences of item i
    public List<Item> slots = new List<Item>();
    private ItemDatabase database;
    public bool showGUI;
    public bool showTooltip;
    private string tooltip;
    private bool dragging;
    private Item itemBuffer;
    private int countBuffer;
    private int oldIndex;

    public Inventory(int w, int h, int m, int s)
    {
        height = h;
        width = w;
        margin = m;
    }

    void Start ()
    {
        width = 5;
        height = 3;
        margin = 20;
        boxSize = 60;
        for (int i = 0; i < width*height; i++)
        {
            slots.Add(new Item());
            inventory.Add(new Item());
            counts.Add(0);
        }
        database = GameObject.FindGameObjectWithTag("Database").GetComponent<ItemDatabase>();
        AddItem(0, 2);
        AddItem(2, 40);
        AddItem(1, 1);
        AddItem(2, 40);
        RemoveItem(2, 31);
        RemoveItem(0, 1);

    }

    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            showGUI = !showGUI;
        }
    }

    void OnGUI()
    {
        tooltip = "";
        if (showGUI)
        {
            DrawInventory();
        }
        if (showTooltip)
        {
            GUI.Box(new Rect(Event.current.mousePosition.x,Event.current.mousePosition.y,200,200), tooltip);
        }
        if (dragging)
        {
            GUI.DrawTexture(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, boxSize, boxSize), itemBuffer.itemIcon);
        }
    }

    void DrawInventory()
    {
        Event e = Event.current;
        int offsetX = (Screen.width / 2) - ((width * (boxSize + margin)) - margin) / 2;
        int offsetY = (Screen.height / 2) - ((height * (boxSize + margin)) - margin) / 2;
        int yMargin = 0;
        int n = 0;
        for (int j = 0; j < height; j++)
        {
            int xMargin = 0;
            for (int i = 0; i < width; i++)
            {
                Rect slotRect = new Rect(offsetX + (i * boxSize) + xMargin, offsetY + (j * boxSize) + yMargin, boxSize, boxSize);
                GUI.Box(slotRect, "");
                slots[n] = inventory[n];
                if (slots[n].itemName != null)
                {
                    GUI.DrawTexture(slotRect, slots[n].itemIcon);
                    GUI.Label(slotRect, counts[n].ToString());
                    if (slotRect.Contains(e.mousePosition))
                    {
                        CreateTooltip(slots[n], counts[n]);
                        showTooltip = true;
                        if (e.button == 0 && e.type == EventType.mouseDrag && !dragging)
                        {
                            dragging = true;

                            itemBuffer = slots[n];
                            countBuffer = counts[n];
                            oldIndex = n;
                            inventory[n] = new Item();
                            counts[n] = 0;
                        }
                        if (e.type == EventType.mouseUp && dragging)
                        {
                            inventory[oldIndex] = inventory[n];
                            counts[oldIndex] = counts[n];

                            inventory[n] = itemBuffer;
                            counts[n] = countBuffer;

                            dragging = false;
                            itemBuffer = null;
                        }
                    }
                }
                else
                {
                    if (slotRect.Contains(e.mousePosition) && e.type == EventType.mouseUp && dragging)
                    {
                        inventory[n] = itemBuffer;
                        counts[n] = countBuffer;

                        dragging = false;
                        itemBuffer = null;
                    }
                    if (false) //hover over no slot
                    {
                        inventory[oldIndex] = itemBuffer;
                        counts[oldIndex] = countBuffer;
                    }
                }
                
                if (tooltip == "")
                {
                    showTooltip = false;
                }
                xMargin += margin;
                n++;
            }
            yMargin += margin;
        }
    }

    void CreateTooltip(Item item, int occurences)
    {
        tooltip = item.itemName + " (" + occurences.ToString() + ") " + item.itemType.ToString() + "\n\n" + item.itemDesc;
    }

    public int AddItem(int id, int amount) //returns amount of Items not taken
    {
        int count = amount;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemName == null)
            {
                inventory[i] = database.items[id];
                if (database.items[id].itemMaxCount >= count)
                {
                    counts[i] = count;
                    return 0;
                }
                else
                {
                    counts[i] = inventory[id].itemMaxCount;
                    count -= inventory[id].itemMaxCount;
                }
            }

            else if (inventory[i].itemID == id)
            {
                if(inventory[id].itemMaxCount - counts[i] >= count)
                {
                    counts[i] += count;
                    return 0;
                }
                else
                {
                    counts[i] = inventory[id].itemMaxCount;
                    count -= counts[i]-count;
                }
            }
        }
        return count;
    }

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
}
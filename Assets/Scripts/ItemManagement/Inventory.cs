using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {
    
    private ItemDatabase database;
    private Container bar, grid, transfer;

    //layout sizes (better be global)
    static int margin = 20;
    static int spacing = 50;

    //pick and drop
    private Slot holding;
    private bool isHolding;
    private bool shift;

    //swap vars
    private Container oldContainer;
    private int oldNo;

    //inventory lock
    private bool locked;

    //Visibility
    private static bool visible;

    void Awake ()
    {
        database = FindObjectOfType<ItemDatabase>();
        visible = false;
        holding = new Slot();
        holding.GetTransform().SetParent(GameObject.FindGameObjectsWithTag("Canvas")[0].transform);
        holding.GetTransform().SetAsLastSibling();

        bar = new Container(5,1,new Vector2(0.5f,0.025f), this, true);
        grid = new Container(10, 4, new Vector2(0.5f, 0.7f), this, false);
        grid.SetVisible(false);

        if (!AddItem(bar, 0, 188) || !AddItem(bar, 1, 18)) Debug.LogError("Noooo");
    }

    public static bool isVisible() {
        return visible;
    }

    public bool ItemTransfer(Container from, Container to, int itemID, int count) {
        if (!from.Remove(itemID, count) || !to.Add(itemID, count)) return false;
        from.Redraw();
        to.Redraw();
        return true;
    }

    public bool AddItem(Container c, int itemID, int count) {
        if (!c.Add(itemID, count)) return false;
        c.Redraw();
        return true;
    }

    public bool RemoveItem(Container c, int itemID, int count) {
        if (!c.Remove(itemID, count)) return false;
        c.Redraw();
        return true;
    }

    public void Swap(Container c, int no) {
        Debug.Log(no);
        if(c.GetSlot(no).getItem() != null && oldContainer == null) {
            if (shift) {
                if (c == bar) {
                    if (!c.MoveTo(no, grid)) Debug.Log("Nein");
                    return;
                } else if(c == grid) {
                    if(!c.MoveTo(no, bar)) Debug.Log("Nein");
                    return;
                }
            } else {
                oldContainer = c;
                oldNo = no;
                holding.CopyFrom(c.GetSlot(no));
                c.GetSlot(no).EmptySlot();
                isHolding = true;
            }
        } else if (!(oldContainer == c && oldNo == no)) {
            oldContainer.GetSlot(oldNo).CopyFrom(c.GetSlot(no));
            c.GetSlot(no).CopyFrom(holding);
            holding.EmptySlot();
            oldContainer = null;
            isHolding = false;
            c.Redraw();
            return;
        }
        oldContainer = c;
        oldNo = no;
    }

    void Update() {
        if (Input.GetButtonDown("Inventory")) {
            visible = !visible;
            grid.SetVisible(visible);

            if (oldContainer != null && !visible) {
                oldContainer.GetSlot(oldNo).CopyFrom(holding);
                holding.EmptySlot();
                oldContainer = null;
            }
        }
        if (isHolding) {
            holding.GetTransform().position = Input.mousePosition;
        }
        if (Input.GetKey(KeyCode.LeftShift)) {
            shift = true;
        } else {
            shift = false;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) bar.ScrollForward();
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) bar.ScrollBackward();
        }
    }
}
    
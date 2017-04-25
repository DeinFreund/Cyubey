using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Container {

    private Inventory owner;
    private GameObject container, slotWrapper, buttonWrapper;

    private int width, height;
    private Slot[] slots;
    private GameObject[] buttons;

    // Styleset
    private int margin = 50;
    private int spacing = 20;
    private int slotSize = 50;

    //scroll property
    private bool canScroll;
    private int scrollPositon;

    // Use this for initialization
    public Container(int width, int height, Vector2 anchor, Inventory owner, bool scroll) {
        this.owner = owner;
        canScroll = scroll;

        //Setting up container
        container = new GameObject("Container");
        container.transform.parent = GameObject.FindGameObjectsWithTag("Canvas")[0].transform;
        container.transform.SetAsFirstSibling();

        //Setting up the slot wrapper
        container.AddComponent<CanvasGroup>();
        container.AddComponent<RectTransform>();
        container.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        container.GetComponent<RectTransform>().anchorMin = anchor;
        container.GetComponent<RectTransform>().anchorMax = anchor;
        container.GetComponent<RectTransform>().pivot = anchor;

        slotWrapper = new GameObject("Slots");
        slotWrapper.transform.parent = container.transform;
        slotWrapper.AddComponent<CanvasGroup>().blocksRaycasts = false;
        slotWrapper.AddComponent<RectTransform>();
        slotWrapper.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        slotWrapper.GetComponent<RectTransform>().sizeDelta = new Vector2(width * (slotSize + spacing) - spacing, height * (slotSize + spacing) - spacing);
        slotWrapper.GetComponent<RectTransform>().anchorMin = anchor;
        slotWrapper.GetComponent<RectTransform>().anchorMax = anchor;
        slotWrapper.GetComponent<RectTransform>().pivot = anchor;
        slotWrapper.AddComponent<GridLayoutGroup>();
        slotWrapper.GetComponent<GridLayoutGroup>().cellSize = new Vector2(slotSize, slotSize);
        slotWrapper.GetComponent<GridLayoutGroup>().spacing = new Vector2(spacing, spacing);

        buttonWrapper = new GameObject("Background");
        buttonWrapper.transform.parent = container.transform;
        buttonWrapper.transform.SetAsFirstSibling();
        buttonWrapper.AddComponent<RectTransform>();
        buttonWrapper.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        buttonWrapper.GetComponent<RectTransform>().sizeDelta = new Vector2(width * (slotSize + spacing) - spacing, height * (slotSize + spacing) - spacing);
        buttonWrapper.GetComponent<RectTransform>().anchorMin = anchor;
        buttonWrapper.GetComponent<RectTransform>().anchorMax = anchor;
        buttonWrapper.GetComponent<RectTransform>().pivot = anchor;
        buttonWrapper.AddComponent<GridLayoutGroup>();
        buttonWrapper.GetComponent<GridLayoutGroup>().cellSize = new Vector2(slotSize, slotSize);
        buttonWrapper.GetComponent<GridLayoutGroup>().spacing = new Vector2(spacing, spacing);

        slots = new Slot[width * height];
        buttons = new GameObject[width * height];
        this.width = width;
        this.height = height;

        for (int i = 0; i < width * height; i++) {
            int no = i;
            Container c = this;
            buttons[i] = new GameObject("Background");
            buttons[i].AddComponent<Button>();
            buttons[i].AddComponent<Image>().sprite = Resources.Load<Sprite>("items/background");
            buttons[i].GetComponent<Button>().onClick.AddListener(delegate { owner.Swap(c, no); });
            buttons[i].transform.SetParent(buttonWrapper.transform);

            slots[i] = new Slot();
            slots[i].GetTransform().SetParent(slotWrapper.transform);
        }
        if (scroll) buttons[0].GetComponent<Image>().sprite = Resources.Load<Sprite>("items/selection");
    }
    /*
        anchorX
        0--1--2/0
        |   |   |
        *  1/1  1 anchorY
        |   |   |
        0/2-*---2
    */

    public void SetVisible(bool isVisible) {
        if (isVisible) {
            container.GetComponent<CanvasGroup>().alpha = 1f;
            container.GetComponent<CanvasGroup>().blocksRaycasts = true;
        } else {
            container.GetComponent<CanvasGroup>().alpha = 0f;
            container.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void ScrollForward() {
        buttons[scrollPositon % GetSize()].GetComponent<Image>().sprite = Resources.Load<Sprite>("items/background");
        scrollPositon = (scrollPositon + 1) % GetSize();
        buttons[scrollPositon % GetSize()].GetComponent<Image>().sprite = Resources.Load<Sprite>("items/selection");
    }

    public void ScrollBackward() {
        buttons[scrollPositon].GetComponent<Image>().sprite = Resources.Load<Sprite>("items/background");
        scrollPositon = (scrollPositon - 1 + GetSize()) % GetSize();
        buttons[scrollPositon].GetComponent<Image>().sprite = Resources.Load<Sprite>("items/selection");
    }

    public GameObject GetContainer() {
        return slotWrapper;
    }

    public Slot GetSlot(int no) {
        if (no < 0 || no + 1 > width * height) Debug.Log("Wub");
        return slots[no];
    }

    public void SetSlot(int no, Slot slot) {
        slots[no] = slot;
    }

    public int GetSize() {
        return width * height;
    }

    public bool Has(int itemID, int count) {
        int amount = count;
        for (int i = 0; i < GetSize(); i++) {
            if (slots[i].getItem() == Object.FindObjectOfType<ItemDatabase>().ItemFromID(itemID)) {
                if (slots[i].getCount() >= amount) {
                    return true;
                } else {
                    amount -= slots[i].getCount();
                }
            }
        }
        return false;
    }

    //checks if the container has enough space for count occurences of itemID
    public bool HasSpaceFor(int itemID, int count) {
        int amount = count;
        for(int i = 0; i < GetSize(); i++) {
            if(slots[i].getItem() == null) {
                if(amount <= 99) {
                    return true;
                } else {
                    amount -= 99;
                }
            } else if(slots[i].getItem().getID() == itemID) {
                if(slots[i].getCount() + amount <= 99) {
                    return true;
                } else {
                    amount -= (99 - slots[i].getCount());
                }
            }
        }
        return false;
    }

    public bool Add(int itemID, int count) {
        if (!HasSpaceFor(itemID, count)) { Debug.Log("add failed"); return false; }
        int amount = count;
        for (int i = 0; i < GetSize(); i++) {
            if (slots[i].getItem() == null) {
                if (amount <= 99) {
                    slots[i].SetItem(itemID, amount);
                    return true;
                } else {
                    amount -= 99;
                    slots[i].SetItem(itemID,99);
                }
            } else if (slots[i].getItem().getID() == itemID) {
                if (slots[i].getCount() + amount <= 99) {
                    slots[i].addCount(amount);
                    return true;
                } else {
                    amount -= (99 - slots[i].getCount());
                    slots[i].setCount(99);
                }
            }
        }
        return false;
    }

    public bool Remove(int itemID, int count) {
        if (!Has(itemID, count)) { Debug.Log("remove failed"); return false; }
        int amount = count;
        Item item = Object.FindObjectOfType<ItemDatabase>().ItemFromID(itemID);
        for (int i = 0; i < GetSize(); i++) {
            if (slots[i].getItem() == item) {
                if (slots[i].getCount() >= amount) {
                    slots[i].removeCount(amount);
                    return true;
                } else {
                    amount -= slots[i].getCount();
                    slots[i].EmptySlot();
                }
            }
        }
        return false;
    }

    public bool Give(int itemID, int count, Container other) {
        if (Has(itemID, count) && other.HasSpaceFor(itemID, count)) {
            other.Add(itemID, count);
            Remove(itemID, count);
            return true;
        }
        Debug.Log("give failed");
        return false;
    }

    public bool MoveTo(int no, Container other) {
        for(int i = 0; i < other.GetSize(); i++) {
            if(other.GetSlot(i).getItem() == null) {
                other.GetSlot(i).CopyFrom(GetSlot(no));
                GetSlot(no).EmptySlot();
                return true;
            }
        }
        return false;
    }

    //Updates Slot graphics
    public void Redraw() {
        for (int i = 0; i < width * height; i++) {
            if (slotWrapper == null) Debug.Log("how?!");
            slots[i].GetTransform().SetParent(slotWrapper.transform);
            slots[i].GetTransform().SetAsLastSibling();
        }
    }

    //saves content of a container
    void Save() {

    }

    //restores content of a container
    void Restore() {

    }
}

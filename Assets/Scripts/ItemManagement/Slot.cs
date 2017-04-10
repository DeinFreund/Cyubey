using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Slot {

    private Item item;
    private int count;

    public GameObject image;
    private GameObject text;

    public Slot()
    {
        image = new GameObject();
        text = new GameObject();
        text.transform.SetParent(image.transform);
        image.AddComponent<Image>();
        image.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(50, 50);
        text.AddComponent<Text>();
        image.AddComponent<CanvasGroup>().blocksRaycasts = false;
        text.transform.localPosition = new Vector2(0, 0);
        text.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.GetComponent<Text>().rectTransform.sizeDelta = new Vector2(50, 50);
        text.GetComponent<Text>().fontSize = 14;
        EmptySlot();
    }

    public void SetItem(int itemID, int count) {
        item = Object.FindObjectOfType<ItemDatabase>().ItemFromID(itemID);
        if (item == null) Debug.Log(itemID.ToString() + " not set");
        this.count = count;
        text.name = count.ToString();
        image.name = item.itemName;
        image.GetComponent<Image>().sprite = item.getIcon();
        text.GetComponent<Text>().text = count.ToString();
    }

    public void EmptySlot() {
        item = null;
        image.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/empty");
        text.name = "No Text";
        image.name = "Empty";
        text.GetComponent<Text>().text = "";
    }

    public void CopyFrom(Slot other) {
        if(other.getItem() != null) {
            SetItem(other.getItem().itemID, other.getCount());
        } else {
            EmptySlot();
        }
    }

    public Transform GetTransform() {
        return image.transform;
    }

    public GameObject getText()
    {
        return text;
    }

    public Item getItem()
    {
        return item;
    }

    public void setItem(Item newItem) {
        item = newItem;
    }

    public int getCount()
    {
        return count;
    }

    public void addCount(int newCount)
    {
        count += newCount;
    }

    public void removeCount(int newCount)
    {
        count -= newCount;
    }

    public void setCount(int newCount)
    {
        count = newCount;
    }
}

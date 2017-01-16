using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Slot {

    private Item item;
    private int count;
    private Image sprite;
    private GameObject text;

	public Slot(Item newItem, int newCount)
    {
        item = newItem;
        count = newCount;
        sprite = GameObject.Instantiate<Image>(Resources.Load<Image>("items/dummy"));
        sprite.sprite = item.getIcon();
        text = new GameObject("Count");
        text.AddComponent<Text>();
        text.AddComponent<CanvasGroup>().blocksRaycasts = false;
        text.transform.localPosition = new Vector2(0.01f, -0.05f);
        text.transform.localScale = new Vector2(0.01f, 0.01f);
        text.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.GetComponent<Text>().fontSize = 20;
    }

    public Slot() //for empty Slots
    {
        item = null;
        count = 0;
        sprite = GameObject.Instantiate<Image>(Resources.Load<Image>("items/dummy"));
        sprite.sprite = Resources.Load<Sprite>("items/empty");
        text = new GameObject();
        text.AddComponent<Text>();
        text.AddComponent<CanvasGroup>();
        text.GetComponent<CanvasGroup>().blocksRaycasts = false;
        text.transform.localPosition = new Vector2(0.01f, -0.05f);
        text.transform.localScale = new Vector2(0.01f, 0.01f);
        text.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.GetComponent<Text>().fontSize = 20;
    }

    public GameObject getText()
    {
        return text;
    }

    public Item getItem()
    {
        return item;
    }

    public void setItem(Item newItem)
    {
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

    public void setCount(int newCount)
    {
        count = newCount;
    }

    public Image getSprite()
    {
        return sprite;
    }

    public void setSprite(Image newSprite)
    {
        sprite = newSprite;
    }
}

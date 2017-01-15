using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Slot {

    private Item item;
    private int count;
    private Image sprite;
    private Text display;

	public Slot(Item newItem, int newCount)
    {
        item = newItem;
        count = newCount;
        sprite = GameObject.Instantiate<Image>(Resources.Load<Image>("items/dummy"));
        sprite.sprite = item.getIcon();
        sprite.gameObject.AddComponent<Text>().text = count.ToString();
    }

    public Slot() //for empty Slots
    {
        item = null;
        count = 0;
        sprite = GameObject.Instantiate<Image>(Resources.Load<Image>("items/dummy"));
        sprite.sprite = Resources.Load<Sprite>("items/empty");
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

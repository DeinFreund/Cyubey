using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Bar : MonoBehaviour {

    public Inventory inv;
    public RectTransform ParentPanel;
    CanvasGroup canvas;
    private int width, height, bar;
    private int space;
    private int offsetX;
    private int slotSize = 50;
    private GameObject[] buttons;

    // Use this for initialization
    void Start () {
        canvas = ParentPanel.GetComponent<CanvasGroup>();
        print("Bar start");
        width = inv.getWidth();
        height = inv.getHeight();
        bar = inv.getBarSize();
        space = (int)((double)slotSize / 3);
        offsetX = (int)((double)(ParentPanel.rect.width - (slotSize + space) * (bar - 1)) / 2);

        print(width + " " + height + " " + bar);

        buttons = new GameObject[bar];

        Image cur;

        for (int i = 0; i < bar; i++)
        {
            int n = width * height + i;
            GameObject button = new GameObject();
            buttons[i] = button;
            button.AddComponent<Button>();
            button.AddComponent<Image>();
            button.transform.SetParent(ParentPanel, false);
            button.transform.localScale = new Vector2(0.5f, 0.5f);
            button.transform.localPosition = new Vector2(offsetX + i * (space + slotSize), 50);
            button.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/background");
            button.GetComponent<Button>().onClick.AddListener(() => inv.itemMove(n));
            button.transform.SetSiblingIndex(1);

            cur = inv.getSlot(n).getSprite();
            cur.transform.SetParent(ParentPanel, false);
            cur.transform.localScale = new Vector3(slotSize, slotSize, 1);
            cur.transform.localPosition = new Vector3(offsetX + i * (space + slotSize), 50);
            cur.transform.SetSiblingIndex(2);
        }
    }
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < bar; i++)
        {
            inv.getSlot(width*height + i).getSprite().transform.localPosition = new Vector2(offsetX + i * (space + slotSize), 50);
            inv.getSlot(width * height + i).getSprite().transform.SetParent(GetComponent<CanvasGroup>().transform);
        }
    }
}

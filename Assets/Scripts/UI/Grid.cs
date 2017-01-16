using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Grid : MonoBehaviour
{
    public Inventory inv;

    private int slotSize = 50;
    private int space;
    private int offsetX, offsetY;

    public RectTransform ParentPanel;
    private int width, height;
    private bool visible;
    private GameObject[] buttons;

    // Use this for initialization
    void Start()
    {
        CanvasGroup canvas = ParentPanel.GetComponent<CanvasGroup>();
        canvas.transform.SetSiblingIndex(0);

        hide();

        width = inv.getWidth();
        height = inv.getHeight();
        space = (int)((double)slotSize / 3);
        offsetX = (int)((double)(ParentPanel.rect.width - (slotSize + space) * (width - 1)) / 2);
        offsetY = (int)((double)(ParentPanel.rect.height - (slotSize + space) * (height - 1)) / 2);

        buttons = new GameObject[width * height];

        Image cur;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int n = j + i * width;

                GameObject button = new GameObject();
                buttons[n] = button;
                button.AddComponent<Button>();
                button.AddComponent<Image>();
                button.transform.SetParent(ParentPanel, false);
                button.transform.localScale = new Vector2(0.5f, 0.5f);
                button.transform.localPosition = new Vector2(offsetX + j * (space + slotSize), (int)ParentPanel.rect.height - offsetY - i * (space + slotSize));
                button.GetComponent<Image>().sprite = Resources.Load<Sprite>("items/background");
                button.GetComponent<Button>().onClick.AddListener(() => inv.itemMove(n));
                button.transform.SetSiblingIndex(1);

                cur = inv.getSlot(n).getSprite();
                cur.transform.SetParent(ParentPanel, false);
                cur.transform.localScale = new Vector2(slotSize, slotSize);
                cur.transform.localPosition = new Vector2(offsetX + j * (space + slotSize),
                    ParentPanel.rect.height - offsetY - i * (space + slotSize));
                cur.transform.SetSiblingIndex(2);

                inv.getSlot(n).getText().transform.SetParent(cur.transform,false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        int space = (int)((double)slotSize / 3);
        int offsetX = (int)((double)(ParentPanel.rect.width - (slotSize + space) * (width - 1)) / 2);
        int offsetY = (int)((double)(ParentPanel.rect.height - (slotSize + space) * (height - 1)) / 2);
        for (int i = 0; i < inv.getHeight(); i++)
        {
            for (int j = 0; j < inv.getWidth(); j++)
            {
                inv.getSlot(j + i * width).getSprite().transform.localPosition = new Vector3(offsetX + j * (space + slotSize),
                    ParentPanel.rect.height - offsetY - i * (space + slotSize), 0);
                inv.getSlot(j + i * width).getSprite().transform.SetParent(GetComponent<CanvasGroup>().transform);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            if(visible)
            {
                hide();
                if(inv.getPick())
                {
                    inv.itemMove(inv.getBuffer());
                }
            }
            else
            {
                show();
            }
        }
        if (inv.getPick())
        {
            inv.getDraging().transform.position = Input.mousePosition;
        }
    }
    
    void hide()
    {
        CanvasGroup canvas = ParentPanel.GetComponent<CanvasGroup>();
        canvas.alpha = 0f;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
        visible = false;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
    }

    void show()
    {
        CanvasGroup canvas = ParentPanel.GetComponent<CanvasGroup>();
        canvas.alpha = 1f;
        canvas.blocksRaycasts = true;
        canvas.interactable = true;
        visible = true;
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InfoFeed : MonoBehaviour {

    public Transform InfoPanel;

    private List<InfoMessage> toremove = new List<InfoMessage>();

    private static List<InfoMessage> feed = new List<InfoMessage>();

    void Update()
    {
        lock (feed)
        {
            feed.RemoveAll(m => m.isTimedOut());
            toremove.Clear();
            float height = 0;
            foreach (InfoMessage msg in feed)
            {
                if (msg.panel == null)
                {
                    msg.panel = Instantiate(InfoPanel).gameObject;
                    msg.panel.transform.SetParent(GameObject.Find("Canvas").transform, false);
                    msg.panel.transform.Find("Text").GetComponent<Text>().text = msg.message;
                }
                msg.panel.GetComponent<RectTransform>().anchoredPosition = (msg.panel.GetComponent<RectTransform>().anchoredPosition * 4 + new Vector2(0, height) - msg.panel.GetComponent<RectTransform>().sizeDelta / 2f) / 5f;
                height -= msg.panel.GetComponent<RectTransform>().sizeDelta.y;
            }
        }
    }

    public static void displayMessage(InfoMessage message)
    {
        lock (feed)
        {
            Debug.Log("Feed: " + message.message);
            feed.Insert(0, message);
        }
    }



}

public class InfoMessage
{
    public string message;
    float timeout;
    public GameObject panel;

    public InfoMessage(string message, float timeout)
    {
        this.message = message;
        this.timeout = timeout;
    }

    public InfoMessage(string message) : this(message, float.MaxValue)
    {

    }

    public virtual bool isTimedOut()
    {
        return Time.time > timeout;
    }
}
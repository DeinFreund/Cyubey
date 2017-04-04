using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuActions : MonoBehaviour
{

    public static ConcurrentQueue<string> events = new ConcurrentQueue<string>();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        string name;
        while (events.TryDequeue(out name))
        {
            transform.SendMessage(name);
        }
    }

    void hostGame()
    {
        if (ServerNetworkManager.hostGame())
        {
            transform.Find("MenuPanel").gameObject.SetActive(false);
            transform.Find("LoginPanel").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("PopupPanel").gameObject.SetActive(true);
            GameObject.Find("textPopup").GetComponent<Text>().text = "Error creating server";
        }
    }
    void joinGame()
    {
        if (ClientNetworkManager.joinGame(GameObject.Find("textIP").GetComponent<Text>().text))
        {
            transform.Find("MenuPanel").gameObject.SetActive(false);
            transform.Find("LoginPanel").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("PopupPanel").gameObject.SetActive(true);
            GameObject.Find("textPopup").GetComponent<Text>().text = "Error joining server";
        }
    }

    void connected()
    {

        transform.Find("NetworkPanel").gameObject.SetActive(false);
        transform.Find("LoginPanel").gameObject.SetActive(true);
    }

    void connectError()
    {

        transform.Find("PopupPanel").gameObject.SetActive(true);
        GameObject.Find("textPopup").GetComponent<Text>().text = "Connection error";
    }

    void login()
    {
        ClientNetworkManager.login(GameObject.Find("textUsername").GetComponent<Text>().text, GameObject.Find("textPassword").GetComponent<Text>().text);
    }

    void loginError()
    {
        transform.Find("PopupPanel").gameObject.SetActive(true);
        GameObject.Find("textPopup").GetComponent<Text>().text = "Login failed";
    }

    void loadGame()
    {
        Debug.Log("Loading game");
        SceneManager.LoadScene("cyubey");
    }

}
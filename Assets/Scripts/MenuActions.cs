using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuActions : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void hostGame()
    {
        if (NetworkManager.hostGame())
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
        NetworkManager.joinGame(GameObject.Find("textIP").GetComponent<Text>().text);
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
        NetworkManager.login(GameObject.Find("textUsername").GetComponent<Text>().text, GameObject.Find("textPassword").GetComponent<Text>().text);
    }

    void loginError()
    {
        transform.Find("PopupPanel").gameObject.SetActive(true);
        GameObject.Find("textPopup").GetComponent<Text>().text = "Login failed";
    }

}
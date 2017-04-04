using UnityEngine;
using System.Collections;

public class AccountManager : MonoBehaviour {
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void login(string username, string pw)
    {

    }
}

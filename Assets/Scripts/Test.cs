using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Test : MonoBehaviour {

    // Use this for initialization
    void Start() {
        List<int> list = new List<int>();
        list.Add(1);
        list.Add(2);
        Debug.Log(list.Take(1).First());
        Debug.Log(list.Take(1).First());
        Debug.Log(list.Take(1).First());
    }
	
	// Update is called once per frame
	void Update () {
    }

    
}

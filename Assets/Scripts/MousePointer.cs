using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointer : MonoBehaviour {

    GameObject selector;
    public static Position selection = Position.nil;

	// Use this for initialization
	void Start () {
        selector = GameObject.Find("Selector");
        selector.transform.parent = MovementController.worldParent.transform;
	}

    // Update is called once per frame
    private int cnt;
    private Vector3 check;
	void Update () {
        check = -MovementController.worldParent.transform.position + Camera.main.transform.position + Vector3.right;
        cnt = 100;
        selector.GetComponent<MeshRenderer>().enabled = true;
        while (true && new Position((int)Math.Floor(check.x), (int)Math.Floor(check.y), (int)Math.Floor(check.z)).getBlock().getID() < 0)
        {
            check += Camera.main.transform.forward * 0.3f;
            if (cnt -- < 0)
            {
                selector.GetComponent<MeshRenderer>().enabled = false;
                selection = Position.nil;
                return;
            }
        }
        selection = new Position((int)Math.Floor(check.x), (int)Math.Floor(check.y), (int)Math.Floor(check.z));
        selector.transform.localPosition = selection + Vector3.forward;

        if (Input.GetButtonDown("Fire1"))
        {
            ClientNetworkManager.setBlock(selection, new Air(selection, false));
        }
        if (Input.GetButtonDown("Fire2"))
        {
            ClientNetworkManager.setBlock(selection.above(), new Water(selection.above(), false, 1.0f));
        }
        if (Input.GetButtonDown("Fire3"))
        {
            ClientNetworkManager.setBlock(selection.above(), new Rock(selection.above(), false));
        }
    }
}

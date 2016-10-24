using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderMover : MonoBehaviour {


    GameObject[] blocks;
    List<Coordinates> dirs = new List<Coordinates>();

	// Use this for initialization
	void Start () {
        blocks = new GameObject[10];
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i] = GameObject.Find("Collider" + i);
            blocks[i].GetComponent<MeshRenderer>().enabled = false;
        }
        
        dirs.Add(new Coordinates(0, 1, 0));
        dirs.Add(new Coordinates(0, -1, 0));
        dirs.Add(new Coordinates(0, 0, -1));
        dirs.Add(new Coordinates(0, 0, 1));
        dirs.Add(new Coordinates(1, 0, 0));
        dirs.Add(new Coordinates(-1, 0, 0));
        dirs.Add(new Coordinates(0, 0, -1));
        dirs.Add(new Coordinates(0, 0, 1));
        dirs.Add(new Coordinates(1, 0, 0));
        dirs.Add(new Coordinates(-1, 0, 0));
    }
	
	// Update is called once per frame
	void Update () {

        Position pos = new Position((int)System.Math.Ceiling(transform.position.x), (int)System.Math.Ceiling(transform.position.y), (int)System.Math.Ceiling(transform.position.z) - 1);
        if (!(pos.getBlock()is Air))
        {
            if (!(pos.above().getBlock()is Air)) pos = pos.above();
            else if (!(pos.below().getBlock()is Air)) pos = pos.below();
            else if (!(pos.offset(1,0,0).getBlock()is Air)) pos = pos.offset(1,0,0);
            else if (!(pos.offset(-1, 0, 0).getBlock()is Air)) pos = pos.offset(-1, 0, 0);
            else if (!(pos.offset(0, 0, 1).getBlock()is Air)) pos = pos.offset(0, 0, 1);
            else if (!(pos.offset(0, 0, -1).getBlock()is Air)) pos = pos.offset(0, 0, -1);
        }
        if (pos.getChunk() == null) return;
        int i = 0;
        //Debug.Log(pos);
        Position groundpos = null;
        foreach (Coordinates norm in dirs){
            if (i == 6) pos = groundpos.above();
            Position npos = pos;
            while (npos.getBlock()is Air)
            {
                npos = npos.offset(norm);
            }
            if (i == 1) groundpos = npos;
            //if (i == 1) Debug.Log(npos);
            blocks[i++].transform.position = npos + new Vector3(0, 0, 0);
        }
    }
}

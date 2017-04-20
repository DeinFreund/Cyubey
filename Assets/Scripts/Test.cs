using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

    // Use this for initialization
    void Start() {
        Debug.Log(transform.localToWorldMatrix);

        //ChunkTest();
    }
	
	// Update is called once per frame
	void Update () {

    }

    //Chunk saving and loading test
    void ChunkTest() {
        ChunkManager test = new ChunkManager(2);

        test.SaveChunk(1, 0, 0, new byte[] { 2, 3, 5, 7 });
        test.SaveChunk(0, 1, 0, new byte[] { 9, 11, 13, 17 });
        test.SaveChunk(1, 1, 1, new byte[] { 6,6,6 });
    }
}

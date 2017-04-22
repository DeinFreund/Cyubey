using UnityEngine;
using System.Collections;
using System.Linq;

public class Test : MonoBehaviour {

    // Use this for initialization
    void Start() {
        Debug.Log(transform.localToWorldMatrix);

        //ChunkTest();
        SaveChunk();
    }
	
	// Update is called once per frame
	void Update () {

    }

    void SaveChunk() {
        ChunkManager test = new ChunkManager(2, "chunks/for/me");
        test.SaveChunk(0, -1, 0, new byte[] { 21, 53 });
        test.LoadChunk(0, -1, 0);
    }

    //Chunk saving and loading test
    void ChunkTest() {
        ChunkManager test = new ChunkManager(16, "Chunks");
        int err = 0;
        int cnt = 300;
        Vector3[] co = new Vector3[cnt];
        byte[][] chunks = new byte[cnt][];
        byte[][] saved = new byte[cnt][];

        for (int i = 0; i < cnt; i++) {
            co[i] = Random.insideUnitSphere * 50;
            int bytes = (int)Mathf.Ceil(Random.value * cnt);
            chunks[i] = new byte[bytes];
            for (int j = 0; j < bytes; j++) {
                chunks[i][j] = (byte)(Random.value * 255);
            }
        }
        for (int i = 0; i < cnt; i++) {
            test.SaveChunk((int)co[i].x, (int)co[i].y, (int)co[i].z, chunks[i]);
        }

        for (int i = 0; i < cnt; i++) {
            saved[i] = test.LoadChunk((int)co[i].x, (int)co[i].y, (int)co[i].z);
        }

        for(int i = 0; i < cnt; i++) {
            if(!Enumerable.SequenceEqual(chunks[i], saved[i])) {

                string a = "";
                for(int j = 0; j < chunks[i].Length; j++) {
                    a += chunks[i][j].ToString() + " ";
                }
                a += " expected, but got ";
                for (int j = 0; j < chunks[i].Length; j++) {
                    a += saved[i][j].ToString() + " ";
                }
                Debug.LogError(a);
                err++;
            }
        }
        Debug.Log("Load errors:" + err);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BlockFactory : MonoBehaviour {

    public static Transform[] block;
    public static Mesh[] blockmesh;
    public static Transform chunk;
    public static Transform playerPrefab;
    public static Transform waterPrefab;

    public Transform[] _block;
    public Transform _chunk;
    public Transform _waterPrefab;
    public Transform _playermodel;

    protected static Dictionary<int, HashSet<GameObject> > gopool = new Dictionary<int, HashSet<GameObject> >();
    protected static Dictionary<int, List<Block>> blockpool = new Dictionary<int, List<Block>>();

    public void Awake()
    {
        waterPrefab = _waterPrefab;
        block = _block;
        chunk = _chunk;
        playerPrefab = _playermodel;
        blockmesh = new Mesh[block.Length];
        for (int i = 0; i < _block.Length; i++)
        {
            blockmesh[i] = block[i].GetComponent<MeshFilter>() == null ? null : block[i].GetComponent<MeshFilter>().sharedMesh;
        }
        blockpool.Add(Air.ID, new List<Block>());
        blockpool.Add(Block.ID, new List<Block>());
        for (int i = 0; i < 0*1e6; i++)
        {
            //blockpool[0].Add(new Block());
            //blockpool[-1].Add(new Air());
        }
    }

    float lastDbg = 0;
    static int instantiations = 0;
    static int poolUsages = 0;
    static int poolSize = 0;
    public void Update()
    {
        if (Time.time - lastDbg > 2)
        {
            lastDbg = Time.time;
            //Debug.Log("Block factory stats:\nPool size: " + poolSize + "\nPool usages: " + poolUsages + "\nInstantiations: " + instantiations);
            poolUsages = 0;
            instantiations = 0;
        }
    }

    public static GameObject instantiate(Block block)
    {
        if (!gopool.ContainsKey(block.getMeshID()))
        {
            gopool.Add(block.getMeshID(), new HashSet<GameObject>());
        }
        if (gopool[block.getMeshID()].Count == 0)
        {
            instantiations++;
            GameObject retval = ((Transform)Instantiate(BlockFactory.block[block.getMeshID()], block.getPosition(), Quaternion.identity)).gameObject;
            return retval;
        }
        HashSet<GameObject>.Enumerator it = gopool[block.getMeshID()].GetEnumerator();
        it.MoveNext();
        GameObject go = it.Current;
        it.Dispose();
        gopool[block.getMeshID()].Remove(go);
        go.transform.position = block.getPosition();
        go.SetActive(false);
        go.SetActive(true);
        go.GetComponent<MeshRenderer>().enabled = true;
        poolUsages++;
        poolSize--;
        return go;
    }

    public static void destroy(Block block)
    {
        gopool[block.getMeshID()].Add(block.getGameObject());
        block.getGameObject().GetComponent<MeshRenderer>().enabled = false;
        block.getGameObject().SetActive(false);
        poolUsages++;
        poolSize++;
    }

    public static void unload(Block block)
    {
        blockpool[block.getID()].Add(block);
    }

    private static Block getInstance(int ID, Type blocktype)
    {
        if (blockpool[ID].Count == 0) blockpool[ID].Add((Block)Activator.CreateInstance(blocktype));
        Block block = blockpool[ID][blockpool[ID].Count - 1];
        blockpool[ID].RemoveAt(blockpool[ID].Count - 1);
        return block;
    }

    public static Block create(Coordinates position, bool serverBlock, List<Liquid> liquids)
    {
        Block block;
        switch (TerrainCompositor.GetBlock(position))
        {
            case Rock.ID:
                block = new Rock(position, serverBlock);
                break;
            case Air.ID:
                block = new Air(position, serverBlock);
                break;
            case Water.ID:
                block = new Water(position, serverBlock, 1);
                liquids.Add(block as Liquid);
                break;
            default:
                Debug.LogError("Unknown block ID: " + TerrainCompositor.GetBlock(position) + " at coordinates " + position);
                block = new Air(position, serverBlock);
                break;
        }
        return block;
    }
}

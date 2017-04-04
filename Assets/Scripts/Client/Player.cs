using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player {
    //clientside player

    public readonly int id;
    public readonly string name;
    private bool isActivePlayer = false;
    GameObject gameobject;

    public Player(int id, string name)
    {
        this.id = id;
        this.name = name;
        //ClientNetworkManager.getMyClient().RegisterHandler((short)(Client.CLIENT_MIN + id), updatePlayer);
        gameobject = GameObject.Instantiate(BlockFactory.playerPrefab).gameObject;

    }

    public void setActivePlayer()
    {
        isActivePlayer = true;
        new List<MeshRenderer>(gameobject.GetComponentsInChildren<MeshRenderer>()).ForEach(r => r.enabled = false);
        gameobject.GetComponent<VelocityInterpolator>().enabled = false;
        gameobject.GetComponent<ChunkDrawer>().enabled = true;
        gameobject.GetComponent<MovementController>().enabled = true;
        gameobject.GetComponent<Inventory>().enabled = true;
        gameobject.GetComponentInChildren<Camera>().enabled = true;
        gameobject.GetComponentInChildren<MouseLook>().enabled = true;
        gameobject.GetComponentInChildren<AudioListener>().enabled = true;
        GameObject grid = GameObject.Find("Grid");
        GameObject bar = GameObject.Find("ItemSelection");
        grid.GetComponent<Grid>().inv = gameobject.GetComponent<Inventory>();
        grid.GetComponent<Grid>().Init();
        grid.GetComponent<Grid>().enabled = true;
        bar.GetComponent<Bar>().inv = gameobject.GetComponent<Inventory>();
        bar.GetComponent<Bar>().Init();
        bar.GetComponent<Bar>().enabled = true;
    }


    public void updatePosition(PositionUpdate update)
    {
        if (isActivePlayer) return;
        gameobject.GetComponent<VelocityInterpolator>().updateTransform(update.pos, update.rot, update.vel);
    }

    public override bool Equals(object obj)
    {
        return obj is Player && ((Player)obj).id == id;
    }

    public override int GetHashCode()
    {
        return id;
    }
}

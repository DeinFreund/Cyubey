using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

public class NetworkManager
{

    static int netPort = 12345;
    static bool useNAT = false;
    static public List<Client> clients;
    static private NetworkClient myclient;
    static bool isHost = false;

    public static bool hostGame()
    {
        if (!NetworkServer.Listen(netPort)) return false;
        isHost = true;
        NetworkServer.RegisterHandler(MsgType.Connect, clientConnected);
        Account.loadAccounts();
        clients = new List<Client>();
        myclient = ClientScene.ConnectLocalServer();
        initMyClient();
        return true;
    }

    public static void joinGame(string ip)
    {
        myclient = new NetworkClient();
        initMyClient();
        myclient.Connect(ip, netPort);
    }

    private static void initMyClient()
    {
        myclient.RegisterHandler(MsgType.Connect, connected);
        myclient.RegisterHandler(MsgType.Disconnect, disconnected);
        myclient.RegisterHandler(MsgType.Error, failedConnection);
        myclient.RegisterHandler(Client.LOGIN, loginResponse);
        myclient.RegisterHandler(Client.JOIN, userLoggedIn);
    }

    public static void login(string username, string password)
    {
        Field field = new Field();
        field.addField("name").setString(username);
        field.addField("password").setString(password);
        myclient.Send(Client.LOGIN, new StringMessage(field.serialize()));
    }

    public static bool isServer()
    {
        return isHost;
    }

    //client events
    static void loginResponse(NetworkMessage msg)
    {
        Field data = new Field(msg.ReadMessage<StringMessage>().value);
        if (data.getString().Equals("success"))
        {
            Debug.Log("Loading game");
            SceneManager.LoadScene("cyubey");
        }else
        {
            Debug.Log("Login response: " + data.getString());
            GameObject.Find("GUI").SendMessage("loginError");
        }
    }

    static void connected(NetworkMessage msg)
    {
        GameObject.Find("GUI").SendMessage("connected");

    }
    static void disconnected(NetworkMessage msg)
    {

    }
    static void failedConnection(NetworkMessage msg)
    {
        GameObject.Find("GUI").SendMessage("connectError");
    }

    static void userLoggedIn(NetworkMessage msg)
    {
        Field userdata = new Field(msg.ReadMessage<StringMessage>().value);
        InfoFeed.displayMessage(new InfoMessage(userdata.atField("name").getString() + " logged in", Time.time + 5));
    }

    //server events 

    static void clientConnected(NetworkMessage msg)
    {
        clients.Add(new Client(msg.conn));
    }

}

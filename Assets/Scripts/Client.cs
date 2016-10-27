using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Client {
    public const short LOGIN = 1000; //self login
    public const short JOIN = 1001; // other user authenticated

    private static short idCounter = 100;

    NetworkConnection connection;
    Account acc = null;
    short clientId = idCounter++; 

    public Client(NetworkConnection player)
    {
        this.connection = player;
        player.RegisterHandler(LOGIN, login);
        Debug.Log("New connection from " + player.address);
    }

    public void login(NetworkMessage msg)
    {
        Field data = new Field(msg.ReadMessage<StringMessage>().value);
        if ((acc = Account.getAccount(data.atField("name").getString())) == null)
        {
            acc = Account.registerAccount(data.atField("name").getString(), data.atField("password").getString());
        }
        if (acc.login(this, data.atField("password").getString()))
        {
            send(LOGIN, "success");
            Debug.Log(data.atField("name").getString() + " authenticated");

            foreach (Client c in NetworkManager.clients)
            {
                if (c.acc != null && !c.acc.Equals(acc))
                {
                    c.userLoggedIn(acc);
                    userLoggedIn(c.acc);
                }
                
            }
        }
        else
        {
            acc = null;
            Debug.Log(data.atField("name").getString() + " failed to authenticate");
            send(LOGIN, "failure");
        }
    }

    public void userLoggedIn(Account user)
    {
        Field userdata = new Field();
        userdata.addField("name").setString(user.getName());
        send(JOIN, userdata);
    }

    public void send(short id, Field message)
    {
        connection.Send(id, new StringMessage(message.serialize()));
    }

    public void send(short id, string message)
    {
        Field f = new Field();
        f.setString(message);
        send(id, f);
    }

    public Account getAccount()
    {
        return acc;
    }

    public NetworkConnection getNetworkConnection()
    {
        return connection;
    }
    
    
}

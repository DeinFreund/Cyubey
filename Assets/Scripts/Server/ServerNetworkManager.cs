using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Net;
using System;

public class ServerNetworkManager
{

    public const int NET_PORT = 12345;

    static public List<Client> clients;
    static public List<Player> players;
    static private TcpListener tcpListener;
    static bool isHosting = false;

    public static bool hostGame()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, NET_PORT);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(acceptConnection, tcpListener);

            isHosting = true;
            Account.loadAccounts();
            clients = new List<Client>();
            return ClientNetworkManager.joinGame("127.0.0.1");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Hosting server failed: " + ex);
            return false;
        }
    }

    public static void acceptConnection(IAsyncResult result)
    {
        if (!isHosting) return;
        Debug.Log("Accepting connection");
        try
        {
            lock (clients)
            {
                TcpListener client = (TcpListener)result.AsyncState;
                clients.Add(new Client(client.EndAcceptTcpClient(result)));
                tcpListener.BeginAcceptTcpClient(acceptConnection, tcpListener);
            }
        } catch (Exception e)
        {
            Debug.LogError("Error accepting connection " + e);
        }
    }

    public static void updateBlock(Position pos, Block block)
    {
        if (!isHosting) return;
        Field message = new Field();
        message.addField("pos").setCoordinates(pos);
        message.addField("block").setBytes(ChunkSerializer.serializeBlock(block));
        foreach (Client c in clients)
        {
            c.send(TCPMessageID.UPDATE_BLOCK, message);
        }
    }

    public static bool isServer()
    {
        return isHosting;
    }

    public static void shutdown()
    {
        isHosting = false;
        tcpListener.Stop();
        foreach (Client c in clients)
        {
            c.logout();
        }
        Client.shutdownUDP();
    }
    

}

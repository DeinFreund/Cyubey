using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;

public class ClientNetworkManager
{
    
    static public Dictionary<int, Player> playersByID;
    static private Player myPlayer;
    static private int myPlayerID;
    static private TcpClient tcpClient;
    static private UdpClient udpClient;
    static private NetworkStream connection;
    static private byte[] dataRcvBufTCP = new byte[1024];
    static private IPEndPoint serverEndpoint;


    public static bool joinGame(string ip)
    {
        try
        {
            playersByID = new Dictionary<int, Player>();

            tcpClient = new TcpClient();
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind((tcpClient.Client.LocalEndPoint as IPEndPoint));
            serverEndpoint = new IPEndPoint(IPAddress.Parse(ip), ServerNetworkManager.NET_PORT);
            tcpClient.Connect(serverEndpoint);
            //udpClient.Connect(ip, ServerNetworkManager.NET_PORT);
            connection = tcpClient.GetStream();
            connection.BeginRead(dataRcvBufTCP, 0, dataRcvBufTCP.Length, new AsyncCallback(receiveBytesTCP), null);
            udpClient.BeginReceive(new AsyncCallback(receiveBytesUDP), null);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Joining server failed: " + ex);
            failedConnection();
            return false;
        }
    }


    private static void receiveBytesUDP(IAsyncResult res)
    {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, ServerNetworkManager.NET_PORT);
        byte[] read = udpClient.EndReceive(res, ref endpoint);
        MainThread.runAction(() => receiveUDP(read));
        //Debug.Log("Read " + read.Length + "bytes from " + endpoint + " on client for " + (tcpClient.Client.RemoteEndPoint as IPEndPoint));
        udpClient.BeginReceive(new AsyncCallback(receiveBytesUDP), null);
    }


    private static void receiveBytesTCP(IAsyncResult res)
    {
        int read = connection.EndRead(res);

        if (read > 0)
        {
            byte[] rec = new byte[read];
            Array.Copy(dataRcvBufTCP, rec, read);
            MainThread.runAction(() => receiveTCP(dataRcvBufTCP));
            connection.BeginRead(dataRcvBufTCP, 0, dataRcvBufTCP.Length, new AsyncCallback(receiveBytesTCP), null);
        }
        else
        {
            Debug.LogWarning("Closed TCP Stream");
            tcpClient.Close();
            disconnected();
        }
    }

    private static void receiveUDP(byte[] data)
    {
        try
        {
            //Debug.Log("Received udp from server at " + (tcpClient.Client.RemoteEndPoint as IPEndPoint));
            UDPNetworkMessage message = UDPNetworkMessage.DeserializeObject<UDPNetworkMessage>(data);
            //Debug.Log("UDP Received message of id " + message.getMessageID());
            switch (message.getMessageID())
            {
                case PositionUpdate.ID:
                    lock (playersByID)
                    {
                        PositionUpdate posUpdate = (PositionUpdate)message;
                        playersByID[posUpdate.affectedPlayer].updatePosition(posUpdate);
                    }
                    break;
                default:
                    Debug.LogWarning("Unknown network message with id " + message.getMessageID());
                    break;
            }
        }catch(Exception ex)
        {
            Debug.LogError("Error parsing UDP message " + ex);
        }

    }

    private static void receiveTCP(byte[] data)
    {
        try
        {
            Debug.Log("Received tcp from server at " + (tcpClient.Client.RemoteEndPoint as IPEndPoint));
            Field message = new Field(data);
            Debug.Log("TCPRec=" + message);
            switch ((TCPMessageID)message.getField("messageID").getInt())
            {
                case TCPMessageID.HELLO:
                    connected();
                    break;
                case TCPMessageID.LOGIN_RESPONSE:
                    loginResponse(message);
                    break;
                case TCPMessageID.USER_LOGIN:
                    userLoggedIn(message);
                    break;
                case TCPMessageID.USER_LOGOUT:
                    userLoggedOut(message);
                    break;
                default:
                    Debug.LogWarning("Unknown message " + message);
                    break;
            }
        }
        catch(Exception ex)
        {
            Debug.LogError("Error parsing TCP message " + ex);
        }
    }
    

    public static void login(string username, string password)
    {
        Field field = new Field();
        field.addField("name").setString(username);
        field.addField("password").setString(password);
        send(TCPMessageID.LOGIN_REQUEST, field);
    }

    public static void sendUnreliable(UDPNetworkMessage msg)
    {
        lock (udpClient)
        {
            byte[] message = UDPNetworkMessage.SerializeObject(msg);
            udpClient.Send(message, message.Length, serverEndpoint);
        }
    }

    public static void send(TCPMessageID id, Field message)
    {
        lock (tcpClient)
        {
            message.addField("messageID").setInt((int)id);
            byte[] compressed = message.compress();
            connection.Write(compressed, 0, compressed.Length);
        }
    }
    
    public static TcpClient getMyClient()
    {
        return tcpClient;
    }
    

    public static Player getMyPlayer()
    {
        return myPlayer;
    }

    //client events
    static void loginResponse(Field msg)
    {
        if (msg.getField("Response").getString().Equals("success"))
        {
            myPlayerID = msg.getField("playerID").getInt();
            MenuActions.events.Enqueue("loadGame");
        }
        else
        {
            Debug.Log("Login response: " + msg.getField("Response").getString());
            MenuActions.events.Enqueue("loginError");
        }
    }

    static void connected()
    {

    }
    static void disconnected()
    {

    }
    static void failedConnection()
    {
        MenuActions.events.Enqueue("connectError");
    }

    static void userLoggedIn(Field userdata)
    {
        lock (playersByID) { 
            int id = (short)userdata.getField("id").getInt();
            if (!playersByID.ContainsKey(id))
            {
                Player p = new Player(id, userdata.getField("name").getString());
                InfoFeed.displayMessage(new InfoMessage(p.name + "(" + p.id + ") logged in", Time.time + 5));
                playersByID.Add(id, p);
                if (id == myPlayerID)
                {
                    myPlayer = p;
                    myPlayer.setActivePlayer();
                }
            }
            else
            {
                Debug.LogWarning("Duplicate login for player " + userdata.getField("name").getString());
            }
        }
    }
    static void userLoggedOut(Field userdata)
    {
        lock (playersByID)
        {
            int id = (short)userdata.getField("id").getInt();
            InfoFeed.displayMessage(new InfoMessage(playersByID[id].name + " logged out", Time.time + 5));
            playersByID.Remove(id);
        }
    }


    public static void shutdown()
    {
        udpClient.Close();
        connection.Close();
        tcpClient.Close();
    }
}

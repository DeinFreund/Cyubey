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
    static private byte[] dataRcvBufTCP = new byte[4096];
    static private IPEndPoint serverEndpoint;
    static private HashSet<Coordinates> receivedChunks = new HashSet<Coordinates>();


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
        //MainThread.runAction(() => receiveUDP(read));
        receiveUDP(read);
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
                    MainThread.runAction(() =>
                    {
                        lock (playersByID)
                        {
                            PositionUpdate posUpdate = (PositionUpdate)message;
                            if (playersByID.ContainsKey(posUpdate.affectedPlayer)) playersByID[posUpdate.affectedPlayer].updatePosition(posUpdate);
                        }
                    });
                    break;
                case ChunkData.ID:
                    ChunkData chunkData = (ChunkData)message;
                    receiveChunk(chunkData);
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
            //Debug.Log("Received tcp from server at " + (tcpClient.Client.RemoteEndPoint as IPEndPoint));
            Field message = new Field(data);
            //Debug.Log("Client Received message of type " + ((TCPMessageID)message.getField("messageID").getInt()) + "\n" + message);
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
                case TCPMessageID.CHECK_HASH:
                    checkHash(message);
                    break;
                case TCPMessageID.UPDATE_BLOCK:
                    updateBlock(message);
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

    private static void checkHash(Field message)
    {
        Coordinates coords = message.getField("coords").getCoordinates();
        if (World.getChunk(coords) != null && World.getChunk(coords).hash() != message.getField("hash").getBytes())
        {
            Debug.LogWarning("Chunk " + coords + " out of sync");
            requestChunk(coords);
        }
    }

    private static void receiveChunk(ChunkData chunkData)
    {
        lock (receivedChunks)
        {
            Coordinates coords = new Coordinates(chunkData.x, chunkData.y, chunkData.z);
            Debug.Log("Received chunk " + coords);
            receivedChunks.Add(coords);
            if (World.getChunk(coords) != null) World.getChunk(coords).deserialize(chunkData.chunkData, chunkData.chunkData.Length);
            else Debug.Log("Received chunk " + coords + " not loaded");
        }
    }

    //server updates a block to client
    private static void updateBlock(Field message)
    {
        if (ServerNetworkManager.isServer()) return;
        Position pos = message.getField("pos").getCoordinates();
        Block block = ChunkSerializer.deserializeBlock(message.getField("block").getBytes(), false);
        if (pos.getChunk() != null) MainThread.runAction(() => pos.getChunk().setBlock(pos, block));
    }

    //client requests block update to server
    public static void setBlock(Position pos, Block block)
    {
        Field message = new Field();
        message.addField("pos").setCoordinates(pos);
        message.addField("block").setBytes(ChunkSerializer.serializeBlock(block));
        send(TCPMessageID.SET_BLOCK, message);
    }

    public static void requestChunk(Coordinates coords)
    {
        lock (receivedChunks)
        {
            Debug.Log("Requesting chunk " + coords);
            receivedChunks.Remove(coords);
            Field request = new Field();
            request.addField("coords").setCoordinates(coords);
            send(TCPMessageID.REQUEST_CHUNK, request);
            MainThread.runSoon(() =>
            {
                if (!receivedChunks.Contains(coords)) requestChunk(coords);
            });
        }
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

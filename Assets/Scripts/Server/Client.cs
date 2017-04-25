using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;

public class Client {
    //serverside player

    TcpClient tcpClient;
    IPEndPoint clientEndpoint;
    static UdpClient udpClient;
    Account acc = null;
    PositionUpdate transform = null;
    Account _accLogin = null;
    NetworkStream connection;

    private byte[] dataRcvBufTCP = new byte[1024];

    static Client()
    {

        udpClient = new UdpClient();
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, ServerNetworkManager.NET_PORT));
        udpClient.BeginReceive(new AsyncCallback(receiveBytesUDP), new object());
    }

    public Client(TcpClient player)
    {
        tcpClient = player;
        clientEndpoint = (tcpClient.Client.RemoteEndPoint as IPEndPoint);
        connection = tcpClient.GetStream();
        connection.BeginRead(this.dataRcvBufTCP, 0,this.dataRcvBufTCP.Length, new AsyncCallback(this.receiveBytesTCP), null);
        send(TCPMessageID.HELLO, new Field());
        Debug.Log("New connection from " + (player.Client.RemoteEndPoint as IPEndPoint).Address);
        Debug.Log(player);
    }

    static void receiveBytesUDP(IAsyncResult res)
    {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, ServerNetworkManager.NET_PORT);
        byte[] read = udpClient.EndReceive(res, ref endpoint);
        udpClient.BeginReceive(new AsyncCallback(receiveBytesUDP), null);
        foreach (Client c in ServerNetworkManager.clients)
        {
            if ((c.tcpClient.Client.RemoteEndPoint as IPEndPoint).Port == endpoint.Port)
            {
                c.receiveUDP(read);
            }
        }
        //Debug.Log("Received udp for client " + endpoint);
    }


    void receiveBytesTCP(IAsyncResult res)
    {
        int read = connection.EndRead(res);

        if (read > 0)
        {
            byte[] rec = new byte[read];
            Array.Copy(dataRcvBufTCP, rec, read);
            receiveTCP(dataRcvBufTCP);
            connection.BeginRead(this.dataRcvBufTCP, 0, this.dataRcvBufTCP.Length, new AsyncCallback(this.receiveBytesTCP), this);
        }
        else
        {
            Debug.LogWarning("Closed TCP Stream");
            logout();
        }
    }

    void receiveUDP(byte[] data)
    {

        try
        {
            //Debug.Log("Received udp for client " + (tcpClient.Client.RemoteEndPoint as IPEndPoint));
            UDPNetworkMessage message = UDPNetworkMessage.DeserializeObject<UDPNetworkMessage>(data);
            //Debug.Log("UDP Received message of id " + message.getMessageID());
            switch (message.getMessageID())
            {
                case PositionUpdate.ID:
                    updatePosition((PositionUpdate)message);
                    break;
                default:
                    Debug.LogWarning("Unknown network message with id " + message.getMessageID());
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing UDP message " + ex);
        }

    }

    void receiveTCP(byte[] data)
    {
        try
        {
            Debug.Log("Received tcp for client " + (tcpClient.Client.RemoteEndPoint as IPEndPoint));
            Field message = new Field(data);
            Debug.Log("Server Received message of type " + ((TCPMessageID)message.getField("messageID").getInt()) + "\n" + message);
            switch ((TCPMessageID)message.getField("messageID").getInt())
            {
                case TCPMessageID.LOGIN_REQUEST:
                    login(message);
                    break;
                case TCPMessageID.READY:
                    ready(message);
                    break;
                case TCPMessageID.REQUEST_CHUNK:
                    requestChunk(message);
                    break;
                case TCPMessageID.REQUEST_HASH:
                    requestHash(message);
                    break;
                case TCPMessageID.SET_BLOCK:
                    setBlock(message);
                    break;
                default:
                    Debug.LogWarning("Unknown message " + message);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing TCP message " + ex);
        }
    }

    void login(Field data)
    {
        if (acc != null)
        {
            Debug.LogWarning("Duplicated login for user " + acc.getName());
        }
        if ((_accLogin = Account.getAccount(data.atField("name").getString())) == null)
        {
            _accLogin = Account.registerAccount(data.atField("name").getString(), data.atField("password").getString());
        }
        Field response = new Field();
        if (_accLogin.login(this, data.atField("password").getString()))
        {
            response.atField("response").setString("success");
            response.atField("playerID").setInt(_accLogin.id);
            Debug.Log(data.atField("name").getString() + " authenticated");
        }
        else
        {
            _accLogin = null;
            response.atField("response").setString("wrong password");
            Debug.Log(data.atField("name").getString() + " failed to authenticate");
        }
        send(TCPMessageID.LOGIN_RESPONSE, response);
    }

    void ready(Field msg)
    {
        acc = _accLogin;
        Debug.Log(acc.getName() + " is ready");
        foreach (Client c in ServerNetworkManager.clients)
        {
            if (c.acc != null)
            {
                Debug.Log("Informing " + c.acc.id + " about account " + acc.id);
                c.userLoggedIn(acc);
                if (!c.acc.Equals(acc))
                {
                    Debug.Log("Informing " + acc.id + " about account " + c.acc.id);
                    userLoggedIn(c.acc);
                }
            }

        }
    }

    void requestChunk(Field message)
    {
        Coordinates coords = message.getField("coords").getCoordinates();
        if (World.getChunk(coords) != null)
        {
            sendUnreliable(new ChunkData(acc.id, coords.x, coords.y, coords.z, World.getChunk(coords).serialize()));
        }
    }

    void requestHash(Field message)
    {
        Coordinates coords = message.getField("coords").getCoordinates();
        byte[] hash;
        if (World.getChunk(coords) != null)
        {
            hash = World.getChunk(coords).hash();
        }else
        {
            hash = new byte[0];
        }
        Field response = new Field();
        response.addField("coords").setCoordinates(coords);
        response.addField("hash").setBytes(hash);
        send(TCPMessageID.CHECK_HASH, response);
    }

    void setBlock(Field message)
    {
        Position pos = message.getField("pos").getCoordinates();
        Block block = ChunkSerializer.deserializeBlock(message.getField("block").getBytes());
        pos.getChunk().setBlock(pos, block);
    }


    public bool isLoggedIn()
    {
        return acc != null;
    }

    public void userLoggedIn(Account user)
    {
        Field userdata = new Field();
        userdata.addField("name").setString(user.getName());
        userdata.addField("id").setInt(user.id);
        send(TCPMessageID.USER_LOGIN, userdata);
    }

    public void userLoggedOut(Account user)
    {
        Field userdata = new Field();
        userdata.addField("id").setInt(user.id);
        send(TCPMessageID.USER_LOGOUT, userdata);
    }

    public void updatePosition(PositionUpdate update)
    {
        if (!isLoggedIn()) return;
        this.transform = update;
        //Debug.Log(acc.id + ": " + (Vector3)update.pos);
        if (acc.id != update.affectedPlayer)
        {
            Debug.LogWarning(acc.getName() + " (" + acc.id + ") tried to spoof position update for " + update.affectedPlayer);
            return;
        }

        foreach (Client c in ServerNetworkManager.clients)
        {
            if (!c.isLoggedIn()) continue;
            c.sendUnreliable(update);
        }
    }

    public void sendUnreliable(UDPNetworkMessage msg)
    {
        lock (udpClient)
        {
            byte[] message = UDPNetworkMessage.SerializeObject(msg);
            udpClient.Send(message, message.Length, clientEndpoint);
        }
    }


    private static int messageCounter = 0;
    public void send(TCPMessageID id, Field message)
    {
        lock (tcpClient)
        {
            message.addField("messageID").setInt((int)id);
            message.addField("messageCounter" + messageCounter++).setInt((int)id);
            Debug.Log("Sent to " + clientEndpoint + ": " + message);
            byte[] compressed = message.compress();
            connection.Write(compressed, 0, compressed.Length);
        }
    }
    

    public Account getAccount()
    {
        return acc;
    }

    public void logout()
    {
        connection.Close();
        tcpClient.Close();
        Account _oldAcc = acc;
        acc = null;
        if (ServerNetworkManager.isServer())
        {
            lock (ServerNetworkManager.clients)
            {
                ServerNetworkManager.clients.Remove(this);
                foreach (Client c in ServerNetworkManager.clients)
                {
                    c.userLoggedOut(_oldAcc);
                }
            }
        }
    }

    public static void shutdownUDP()
    {
        udpClient.Close();
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

[Serializable]
public abstract class UDPNetworkMessage
{
    public readonly int affectedPlayer;

    public UDPNetworkMessage(int affectedPlayer)
    {
        this.affectedPlayer = affectedPlayer;
    }

    public abstract byte getMessageID();
    

    public static byte[] SerializeObject<_T>(_T objectToSerialize)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memStr = new MemoryStream();
        bf.Serialize(memStr, objectToSerialize);
        memStr.Position = 0;
        //return "";
        return memStr.ToArray();
    }

    public static _T DeserializeObject<_T>(byte[] dataStream)
    {
        MemoryStream memStr = new MemoryStream(dataStream);
        memStr.Position = 0;
        BinaryFormatter bf = new BinaryFormatter();
        //bf.Binder = new VersionFixer();
        return (_T)bf.Deserialize(memStr);
    }
}
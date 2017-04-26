using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class PositionUpdate : UDPNetworkMessage
{
    public const byte ID = 1;
    public readonly byte id = ID;

    public override byte getMessageID()
    {
        return id;
    }

    public Vector3Serializer vel;
    public Vector3Serializer pos;
    public QuaternionSerializer rot;

    public PositionUpdate(int affectedPlayer) : base(affectedPlayer)
    {
        vel = new Vector3();
        pos = new Vector3();
        rot = new Quaternion();
    }

    public PositionUpdate(int affectedPlayer, Vector3 position, Quaternion rotation, Vector3 vel) : base(affectedPlayer)
    {
        this.pos = position;
        this.rot = rotation;
        this.vel = vel;
    }
}
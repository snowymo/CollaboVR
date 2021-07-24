using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

namespace NetUtils {
    public struct PayloadActionsSend { 
        public uint objID;
    }
    public struct PayloadActionsRecv {
        public uint objID;
        public byte actionAccepted;
    }

    public struct PayloadModifyObjectSend {
        public uint objID;
        public uint actionType;

        public int modifiedTriangleIndex;
        public Vector3 newPosition;
    }
    public struct PayloadModifyObjectRecv {
        public uint objID;
    }

    public struct PayloadSelectObjectSend {
        public uint objID;
    }
    public struct PayloadSelectObjectRecv {
        public byte actionAccepted;
        public uint objID;
    }

    public struct PayloadDeselectObjectSend {
        public uint objID;
    }
    public struct PayloadDeselectObjectRecv {
        public uint objID;
    }

    public struct PayloadCreateObjectSend {
        public uint objID;
    }
    public struct PayloadCreateObjectRecv {
        public byte actionAccepted;
        public uint objID;
    }


    public struct PacketActionsSend {
        public uint seqID;
        public ulong usrID;
        public float time;

        public uint ackBitfield; // redundant acks

        public uint payloadCount;
        public uint actionType;
        public object[] payloads;
    }

    public struct PacketActionsRecv {
        public uint seqID;
        public ulong usrID;
        public float time;

        public uint ackBitfield; // redundant acks

        public uint payloadCount;
        public uint actionType;
        public object[] payloads;
    }
}




namespace NetUtils {

    public class NetBuffer {
        public byte[] data;
        public int size;
        public int index;

        public static void Init(NetBuffer buffer, byte[] data, int size)
        {
            buffer.data = data;
            buffer.size = size;
            buffer.index = 0;
        }


        public static void WriteByte(NetBuffer buff, byte value)
        {
            Debug.Assert(buff.index + sizeof(byte) <= buff.size);

            buff.data[buff.index] = value;

            buff.index += sizeof(byte);
        }


        public static void WriteUint16(NetBuffer buff, ushort value)
        {
            Debug.Assert(buff.index + sizeof(ushort) <= buff.size);

            short valueHToN = IPAddress.HostToNetworkOrder((short)value);

            buff.data[buff.index] = (byte)((valueHToN & 0xFF00) >> 8);
            buff.data[buff.index + 1] = (byte)((valueHToN & 0x00FF));

            buff.index += sizeof(ushort);
        }

        public static uint ReadUint32(NetBuffer buff)
        {
            Debug.Assert(buff.index + sizeof(uint) <= buff.size);

            uint value = ((uint)buff.data[buff.index]) << 24 |
                         ((uint)buff.data[buff.index + 1]) << 16 |
                         ((uint)buff.data[buff.index + 2]) << 8 |
                         (uint)buff.data[buff.index + 3];

            value = (uint)IPAddress.NetworkToHostOrder((int)value);

            buff.index += sizeof(uint);

            return value;
        }

        public static void WriteVector3(NetBuffer buff, Vector3 value)
        {
            Debug.Assert(buff.index + sizeof(float) * 3 <= buff.size);

            //float[] v0 = {value.x, value.y, value.z};
            //Buffer.BlockCopy(v0, 0, buff.data, buff.index, sizeof(float) * 3);
            byte[] a = BitConverter.GetBytes(value.x);
            byte[] b = BitConverter.GetBytes(value.y);
            byte[] c = BitConverter.GetBytes(value.z);

            byte[] data = buff.data;
            int index = buff.index;

            data[index] = a[0];
            data[index + 1] = a[1];
            data[index + 2] = a[2];
            data[index + 3] = a[3];
            data[index + 4] = b[0];
            data[index + 5] = b[1];
            data[index + 6] = b[2];
            data[index + 7] = b[3];
            data[index + 8] = c[0];
            data[index + 9] = c[1];
            data[index + 10] = c[2];
            data[index + 11] = c[3];

            buff.index += sizeof(float) * 3;


        }

        public static float ReadFloat32(NetBuffer buff)
        {
            Debug.Assert(buff.index + sizeof(float) <= buff.size);

            float value = BitConverter.ToSingle(buff.data, buff.index);

            buff.index += sizeof(float);
            return value;
        }

        public static float[] ReadFloat32MultiTEMP(NetBuffer buff)
        {
            float[] retTemp = new float[(buff.size - buff.index) / sizeof(float)];

            Buffer.BlockCopy(buff.data, buff.index, retTemp, 0, (buff.size - buff.index));

            return retTemp;
        }

        public static Vector3 ReadVector3(NetBuffer buff)
        {
            Debug.Assert(buff.index + sizeof(float) <= buff.size);

            Vector3 value = new Vector3(
                BitConverter.ToSingle(buff.data, buff.index),
                BitConverter.ToSingle(buff.data, buff.index + sizeof(float)),
                BitConverter.ToSingle(buff.data, buff.index + (sizeof(float) * 2))
            );

            buff.index += sizeof(float) * 3;
            return value;
        }

        public static void Advance(NetBuffer buff, int n)
        {
            Debug.Assert(buff.index + n <= buff.size);
            buff.index += n;
        }
    }

}

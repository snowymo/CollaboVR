using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{

    public static GlobalToggle.Configuration StringToConfig(string s)
    {
        if (s.Equals("eyesfree")) {
            return GlobalToggle.Configuration.eyesfree;
        }
        if (s.Equals("sidebyside")) {
            return GlobalToggle.Configuration.sidebyside;
        }
        if (s.Equals("mirror")) {
            return GlobalToggle.Configuration.mirror;
        }
        return GlobalToggle.Configuration.sidebyside;
    }

    public static int ParsetoInt16(byte[] value, int index)
    {
        return BitConverter.ToInt16(value, index) < 0 ? BitConverter.ToInt16(value, index) + 0x10000 : BitConverter.ToInt16(value, index);
    }

    public static int ParsetoSInt16(byte[] value, int index)
    {
        return BitConverter.ToInt16(value, index);
    }

    public static float ParsetoFloat(int number)
    {
        return (float)number / 65535f * 2.0f - 1.0f;
    }

    public static float ParsetoRealFloat(byte[] value, int index)
    {
        return BitConverter.ToSingle(value, index);
    }

    public static int ParsetoUInt16(byte[] value, int index)
    {
        return BitConverter.ToUInt16(value, index);
    }

    public static Color ParsetoColor(byte[] value, int index)
    {
        int r, g, b, a;
        //Debug.Log("color:" + BitConverter.ToUInt16(value, index) + "\t" + (BitConverter.ToUInt16(value, index+2) + 0x10000));
        r = ParsetoUInt16(value, index) >> 8;
        g = ParsetoUInt16(value, index) & 0x00ff;
        b = ParsetoUInt16(value, index + 2) >> 8;
        a = ParsetoUInt16(value, index + 2) & 0x00ff;
        return new Color((float)r / 256f, (float)g / 256f, (float)b / 256f, (float)a / 256f);
    }

    public static List<Vector3> ParsetoVector3s(byte[] value, int index, int size)
    {
        List<Vector3> rst = new List<Vector3>();
        for (int i = 0; i < size; i++) {
            float x = ParsetoFloat(ParsetoInt16(value, index + i * 6)) * 5;
            float y = ParsetoFloat(ParsetoInt16(value, index + i * 6 + 2)) * 5;
            float z = ParsetoFloat(ParsetoInt16(value, index + i * 6 + 4)) * 5;
            rst.Add(new Vector3(x, y, z));
        }
        return rst;
    }

    public static Quaternion ParsetoQuaternion(byte[] value, int index, float scale)
    {
        float x = ParsetoFloat(ParsetoInt16(value, index)) * scale;
        float y = ParsetoFloat(ParsetoInt16(value, index + 2)) * scale;
        float z = ParsetoFloat(ParsetoInt16(value, index + 4)) * scale;
        float w = Mathf.Sqrt(1.0f - x * x - y * y - z * z);
        return new Quaternion(x, y, z, w);
    }
    public static Quaternion ParsetoRealQuaternion(byte[] value, int index, float scale)
    {
        float x = BitConverter.ToSingle(value, index) * scale;
        float y = BitConverter.ToSingle(value, index + 4) * scale;
        float z = BitConverter.ToSingle(value, index + 8) * scale;
        float w = Mathf.Sqrt(1.0f - x * x - y * y - z * z);
        return new Quaternion(x, y, z, w);
    }
    public static Vector3 ParsetoVector3(byte[] value, int index, float scale)
    {
        int ix = ParsetoInt16(value, index);
        float x = ParsetoFloat(ParsetoInt16(value, index)) * scale;
        float y = ParsetoFloat(ParsetoInt16(value, index + 2)) * scale;
        float z = ParsetoFloat(ParsetoInt16(value, index + 4)) * scale;

        return new Vector3(x, y, z);
    }

    public static Vector3 ParsetoRealVector3(byte[] value, int index, float scale)
    {
        int ix = ParsetoInt16(value, index);
        float x = BitConverter.ToSingle(value, index) * scale;
        float y = BitConverter.ToSingle(value, index + 4) * scale;
        float z = BitConverter.ToSingle(value, index + 8) * scale;

        return new Vector3(x, y, z);
    }

    public static Vector2 ParsetoRealVector2(byte[] value, int index, float scale)
    {
        int ix = ParsetoInt16(value, index);
        float x = BitConverter.ToSingle(value, index) * scale;
        float y = BitConverter.ToSingle(value, index + 4) * scale;

        return new Vector2(x, y);
    }

    public static string ParsetoString(byte[] value, int index, int len)
    {
        string ret = "";
        for (int i = 0; i < len / 2; i++) {
            int curbyte = ParsetoInt16(value, index + i * 2);
            int firsthalf = curbyte >> 8;
            int secondhalf = curbyte - ((curbyte >> 8) << 8);
            ret += (char)('A' + (firsthalf - 65));
            ret += (char)('A' + (secondhalf - 65));
        }
        return ret;
    }

    public static int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    public static float SwitchFaceThres = 30;
    public static float SwitchCtrlThres = 60;
    public static float ManipulateRotationMinThres = 0.1f, ManipulateRotationMaxThres=15f;

    public static void Log(string message, Color color)
    {
        Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>",
            (byte)(color.r * 255f), 
            (byte)(color.g * 255f), 
            (byte)(color.b * 255f), 
            message));
    }

    public static Vector3[] BoardToQuad(ChalktalkBoard board)
    {
        Transform tf = board.transform;
        Vector3 pos = tf.position;
        Vector3 dirx = tf.right;
        Vector3 diry = tf.up;
        float bsx = tf.localScale.x * 0.5f;
        float bsy = tf.localScale.y * 0.5f;

        Vector3 vx = dirx * bsx;
        Vector3 vy = diry * bsy;
        return new Vector3[] {
            pos - vx + vy, // TL,
            pos - vx - vy, // BL,
            pos + vx - vy, // BR,
            pos + vx + vy, // TR
        };
    }


    public static int ShortPairToInt(int unshifted, int shifted)
    {
        return unshifted | (shifted << 16);
    }

    public static Color logSuccess = Color.green;
    public static Color logWarning = new Color(255, 127, 80);
    public static Color logError = Color.red;

    public static void Log(int l = 0, Color c = default(Color), string scope="", string details="")
    {
        // TODO: if it is lower than external log level, don't do log
        // TODO: maybe has a map for level and color
        Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}>scope:\t{3}\t{4}</color>",
            (byte)(c.r * 255f),
            (byte)(c.g * 255f),
            (byte)(c.b * 255f), scope, details));
    }

    public static void SetLayer(int layer, GameObject go)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) {
            SetLayer(layer, child.gameObject);
        }
    }

    /// <summary>
    /// color map
    /// </summary>
    public static IDictionary<Color, KeyValuePair<Material, Color>> colorToMaterialInfoMap = new Dictionary<Color, KeyValuePair<Material, Color>>();
    public static IDictionary<Color, KeyValuePair<Material, Color>> colorToMaterialInfoMap2 = new Dictionary<Color, KeyValuePair<Material, Color>>();
}


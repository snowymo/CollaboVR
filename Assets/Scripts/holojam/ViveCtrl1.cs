using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveCtrl1 : Holojam.Tools.SynchronizableTrackable
{
    [SerializeField] string label = "ZH-CTRL1";
    [SerializeField] string scope = "";

    [SerializeField] bool host = true;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          1, 1, 6, 6, 0, false
        );
    }

    // Override Sync()
    protected override void Sync()
    {
        if (Sending)
        {

        }
        else
        {
            //publicData = (byte[])data.bytes.Clone();
            
        }
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        base.Update();
    }

    public Vector3 Pos
    {
        get
        {
            return data.vector3s[0];
        }

    }

    public Quaternion Rot
    {
        get
        {
            return data.vector4s[0];
        }

    }

    public int Grip
    {
        get { return data.ints[1]; }
    }
    public int Trigger
    {
        get { return data.ints[3]; }
    }
}

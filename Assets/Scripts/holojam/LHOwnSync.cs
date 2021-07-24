using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LHOwnSync : Holojam.Tools.SynchronizableTrackable
{

    [SerializeField] string label = "HE-LH";
    [SerializeField] string scope = "";

    [SerializeField] bool host = true;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }
    
    // Override Sync()
    protected override void Sync()
    {
        if (Sending)
        {

        }
        else
        {
            //transform.position = Pos;
            //transform.rotation = Rot;
        }
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        //base.Update();
    }

    public Vector3 Pos
    {
        set
        {
            data.vector3s[0] = value;
        }
        get
        {
            return data.vector3s[0];
        }
    }

    public Quaternion Rot
    {
        set
        {
            data.vector4s[0] = value;
        }
        get
        {
            return data.vector4s[0];
        }
    }

    // You need to reset (allocate) this Controller's data before you can use it
    // Awake() calls ResetData() by default
    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          1, 1
        );
    }

}

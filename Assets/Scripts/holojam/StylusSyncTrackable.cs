using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StylusSyncTrackable : Holojam.Tools.SynchronizableTrackable
{

    [SerializeField] string label = "Stylus";
    [SerializeField] string scope = "";

    [SerializeField] bool host = true;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    public int id = 0;

    public float zOffset = 0.0f;

    public void SetSend(bool b)
    {
        host = b;
    }

    public void ChangeSend()
    {
        host = !host;
    }

    // Override Sync()
    protected override void Sync()
    {
        if (Sending)
        {

        }
        else
        {

        }
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        //base.Update();
    }

    // 0,1,2 means mouse up and down

    public int Data
    {
        get
        {
            return data.ints[0];
        }
        set
        {
            data.ints[0] = value;
        }
    }
    // 3 means wipe aka save
    public int Wipe
    {
        get
        {
            return data.ints[1];
        }
        set
        {
            data.ints[1] = value;
        }
    }

    public int ID
    {
        get
        {
            return id;
        }
        set {
            id = value;
        }
    }

    public Vector3 Pos
    {
        set
        {
            data.vector3s[0] = value;
        }
    }

    public Vector3 Rot
    {
        set
        {
            data.vector3s[1] = value;
        }
    }

    public float ClientDistance
    {
        set {
            data.floats[0] = value;
        }
        get {
            return data.floats[0];
        }
    }

    // You need to reset (allocate) this Controller's data before you can use it
    // Awake() calls ResetData() by default
    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          2, 0, 1, 2, 0, false
        );
        Data = 2;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStylusSync : Holojam.Tools.SynchronizableTrackable
{

    [SerializeField] string label = "ResetSty";
    [SerializeField] string scope = "";

    [SerializeField] bool host = false;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    public void ResetStylus(int id)
    {
        host = true;
        Data = id;
        //owner = true;
    }

    public void ClearOwn()
    {
        owner = false;
    }

    int counter = 0;
    bool owner = false;

    // Override Sync()
    protected override void Sync()
    {
        if (host)
        {
            if(counter == 1)
            {
                counter = 0;
                host = false;
            }
            else
                ++counter;            
        }
        else
        {
            if(Tracked)
                if(GetComponent<StylusSyncTrackable>().ID != Data)
                    GetComponent<StylusSyncTrackable>().SetSend(false);
        }
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        base.Update();
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

    // You need to reset (allocate) this Controller's data before you can use it
    // Awake() calls ResetData() by default
    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 1, 0, false
        );
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDisplaySyncTrackable : Holojam.Tools.SynchronizableTrackable {
    [SerializeField] string label = "DisplayMesh";
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
        if (Sending) {

        }
        else {
            //publicData = (byte[])data.bytes.Clone();
            publicData = data.bytes;
        }
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        base.Update();
    }

    public byte[] publicData;

    //public void GetBytes(out byte[] b) {
    //    if (data.bytes.Length == 0)
    //        return;
    //    b = (byte[])data.bytes.Clone();
    //    //b = data.bytes;
    //    print("data.bytes.Length:" + data.bytes.Length);
    //    print("b.Length:" + b.Length);
    //}
}

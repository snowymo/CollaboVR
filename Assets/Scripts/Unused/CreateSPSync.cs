using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSPSync : Holojam.Tools.SynchronizableTrackable
{

    [SerializeField] string label = "";
    [SerializeField] string scope = "";

    [SerializeField] bool host = true;
    [SerializeField] bool autoHost = false;


    // As an example, allow all the Synchronizable properties to be publicly settable
    // In practice, you probably want to control some or all of these manually in code.

    public void SetLabel(string label) { this.label = label; }
    public void SetScope(string scope) { this.scope = scope; }

    public void SetHost(bool host) { this.host = host; }
    public void SetAutoHost(bool autoHost) { this.autoHost = autoHost; }

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }

    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    public int boardsCnt;

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(0, 0, 0, 1,2);
    }

    //public ResolutionSync requester;

    // Override Sync() to include the scale vector
    protected override void Sync()
    {
        //base.Sync();
        //data.ints[0] = 3;
        if (host)
        {
            data = new Holojam.Network.Flake(0, 0, 0, 1, 2);
            data.ints[0] = boardsCnt;
            Debug.Log("send request to create the " + boardsCnt + " boards");
            host = !host;
        }
        else
        {
            if (Tracked)
            {
                int cnt = ParseSketchpageCnt(data.bytes);
                Debug.Log("confirm chalktalk has " + cnt + " boards and unity has " + boardsCnt + " boards");
            }
            
        }
    }

    int ParseSketchpageCnt(byte[] bytes, int offset = 0)
    {

        if (bytes.Length > 8)
        {
            int cursor = 8 + offset;
            int cnt = Utility.ParsetoInt16(bytes, cursor);
            return cnt;
        }
        return 0;
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        Sync();
    }
}

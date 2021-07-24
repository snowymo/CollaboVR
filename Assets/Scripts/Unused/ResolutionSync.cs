using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionSync : Holojam.Tools.SynchronizableTrackable
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

    public int intCount = 0;

    public override void ResetData()
    {
        host = true;
        intCount = (host) ? 2 : 4;
        //data = new Holojam.Network.Flake(0,0,0,intCount);
        data = new Holojam.Network.Flake(0, 0, 0, 0, intCount);
    }

    //public ResolutionSync requester;

    // Override Sync() to include the scale vector
    protected override void Sync()
    {
        //base.Sync();
        //data.ints[0] = 3;
        if (host)
        {
            if (data.bytes == null)
            {
                return;
            }

            //print("sending resolution sync msg");
            data.bytes[0] = 2;
            data.bytes[1] = 1; // this is the off-by-one byte bug! TODO replace with different Holojam dll?
            host = !host;
        }
        else
        {
            if (data.bytes == null)
            {
                //Debug.Log("NULL");
                return;
            }

            if (data.bytes[0] == 0 || data.bytes[1] == 0) {
                //Debug.Log("IGNORE");
                return;
            }

            Vector2Int res = ParseDisplayInfo(data.bytes);
            GlobalToggleIns.GetInstance().ChalktalkRes = res;
            // disable
//            gameObject.SetActive(false);
        }
    }

    Vector2Int ParseDisplayInfo(byte[] bytes, int offset=0)
    {
        
        if(bytes.Length > 8) {
            int cursor = 8 + offset;
            int resW = Utility.ParsetoInt16(bytes, cursor);
            cursor += 2;
            int resH = Utility.ParsetoInt16(bytes, cursor);
            return new Vector2Int(resW, resH);
        }
        return new Vector2Int(0,0);
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        Sync();
    }
}

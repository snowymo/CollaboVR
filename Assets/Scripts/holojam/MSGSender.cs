using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;

public class MSGSender : Holojam.Tools.SynchronizableTrackable {

    [SerializeField] string label = "MSGSender";
    [SerializeField] string scope = "";

    [SerializeField] bool host = false;
    [SerializeField] bool autoHost = false;

    int curCmdCount;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    void encodeCommand(CommandToServer commandNumber, byte[] parameters)
    {
        byte[] bCN = BitConverter.GetBytes((int)commandNumber);
        byte[] bPN = BitConverter.GetBytes(parameters.Length);

        bMSG = new byte[bCN.Length + bPN.Length + parameters.Length];
        System.Buffer.BlockCopy(bCN, 0, bMSG, 0, bCN.Length);
        System.Buffer.BlockCopy(bPN, 0, bMSG, bCN.Length, bPN.Length);
        System.Buffer.BlockCopy(parameters, 0, bMSG, bCN.Length + bPN.Length, parameters.Length);
    }

    //string msgToSend;
    byte[] bMSG;
    void encodeCommand(CommandToServer commandNumber, int[] parameters)// could be byte array for parameters for future
    {
        // #0 for resolution
        // #1 for reset ownership
        // #2 for creating sketchPage
        // #3 for sending current avatar name
        byte[] bCN = BitConverter.GetBytes((int)commandNumber);
        byte[] bPN = BitConverter.GetBytes(parameters.Length);

        bMSG = new byte[bCN.Length + bPN.Length + bPN.Length * parameters.Length];
        System.Buffer.BlockCopy(bCN, 0, bMSG, 0, bCN.Length);
        System.Buffer.BlockCopy(bPN, 0, bMSG, bCN.Length, bPN.Length);

        for (int i = 0; i < parameters.Length; i++) {
            byte[] bP = BitConverter.GetBytes(parameters[i]);
            System.Buffer.BlockCopy(bP, 0, bMSG, bCN.Length + bPN.Length + i * bP.Length, bP.Length);
        }

    }

    void encodeCommand(CommandToServer commandNumber, string avatarname, string id)// could be byte array for parameters for future
    {
        // #0 for resolution
        // #1 for reset ownership
        // #2 for creating sketchPage
        // #3 for sending current avatar name
        byte[] bCN = BitConverter.GetBytes((int)commandNumber);
        byte[] bP = Encoding.UTF8.GetBytes(avatarname);
        byte[] bPN = BitConverter.GetBytes(bP.Length);
        byte[] bP2 = BitConverter.GetBytes(UInt64.Parse(id));

        bMSG = new byte[bCN.Length + bPN.Length + bP.Length + bP2.Length];
        System.Buffer.BlockCopy(bCN, 0, bMSG, 0, bCN.Length);
        System.Buffer.BlockCopy(bPN, 0, bMSG, bCN.Length, bPN.Length);
        System.Buffer.BlockCopy(bP, 0, bMSG, bCN.Length + bPN.Length, bP.Length);
        System.Buffer.BlockCopy(bP2, 0, bMSG, bCN.Length + bPN.Length + bP.Length, bP2.Length);

    }

    // Override Sync()
    protected override void Sync()
    {
        if (Sending) {

        }
        else {

        }
    }

    [System.Obsolete("This is an obsolete method")]
    public void Send(string msg)
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, msg.Length * sizeof(char), false
        );
        data.bytes = Encoding.ASCII.GetBytes(msg);
        host = true;
    }

    [System.Obsolete("This is an obsolete method")]
    public void Send(CommandToServer cmd, int[] parameters)
    {
        //Debug.Log("send from MSGSender:" + cmd + ":" + Time.time);
        encodeCommand(cmd, parameters);
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, bMSG.Length, false
        );
        data.bytes = bMSG;
        host = true;
    }


    [System.Obsolete("This is an obsolete method")]
    public void Send(CommandToServer cmd, string parameter1, string parameter2)
    {
        //Debug.Log("send from MSGSender:" + cmd + ":" + Time.time);
        encodeCommand(cmd, parameter1, parameter2);
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, bMSG.Length, false
        );
        data.bytes = bMSG;
        host = true;
    }

    void resetDataBytes()
    {
        if (curCmdCount == 0) {
            data = new Holojam.Network.Flake(
              0, 0, 0, 0, 4, false
            );
        }
    }

    public void Add(CommandToServer cmd, byte[] parameters)
    {
        if (!validate(cmd, "byte", parameters.Length)) {
            Debug.LogError("Illegal Parameters!");
            return;
        }
        //Debug.Log("add to bytes from MSGSender:" + cmd);
        encodeCommand(cmd, parameters);
        //int nCmd = BitConverter.ToInt16(data.bytes, 0);
        resetDataBytes();
        ++curCmdCount;
        byte[] bnCmd = BitConverter.GetBytes(curCmdCount);
        Array.Resize(ref data.bytes, data.bytes.Length + bMSG.Length);
        System.Buffer.BlockCopy(bnCmd, 0, data.bytes, 0, bnCmd.Length);
        System.Buffer.BlockCopy(bMSG, 0, data.bytes, data.bytes.Length - bMSG.Length, bMSG.Length);
        host = true;
    }

    public void Add(CommandToServer cmd, int[] parameters)
    {
        if (!validate(cmd, "int", parameters.Length)) {
            Debug.LogError("Illegal Parameters!");
            return;
        }
        //Debug.Log("add to bytes from MSGSender:" + cmd);
        encodeCommand(cmd, parameters);
        //int nCmd = BitConverter.ToInt16(data.bytes, 0);
        resetDataBytes();
        ++curCmdCount;
        byte[] bnCmd = BitConverter.GetBytes(curCmdCount);
        Array.Resize(ref data.bytes, data.bytes.Length + bMSG.Length);
        System.Buffer.BlockCopy(bnCmd, 0, data.bytes, 0, bnCmd.Length);
        System.Buffer.BlockCopy(bMSG, 0, data.bytes, data.bytes.Length - bMSG.Length, bMSG.Length);
        host = true;
    }

    public void Add(CommandToServer cmd, string parameter1, string parameter2)
    {
        if (!validate(cmd, "string", 2)) {
            Debug.LogError("Illegal Parameters!");
            return;
        }
        //Debug.Log("add to bytes from MSGSender:" + cmd);
        encodeCommand(cmd, parameter1, parameter2);
        //int nCmd = BitConverter.ToInt16(data.bytes, 0);
        resetDataBytes();
        ++curCmdCount;
        byte[] bnCmd = BitConverter.GetBytes(curCmdCount);
        Array.Resize(ref data.bytes, data.bytes.Length + bMSG.Length);
        System.Buffer.BlockCopy(bnCmd, 0, data.bytes, 0, bnCmd.Length);
        System.Buffer.BlockCopy(bMSG, 0, data.bytes, data.bytes.Length - bMSG.Length, bMSG.Length);
        host = true;
    }

    bool validate(CommandToServer cmd, string paraType, int paraCount)
    {
        bool isValid = true;
        switch (cmd) {
        case CommandToServer.RESOLUTION_REQUEST:
            isValid = paraCount == 0;
            break;
        case CommandToServer.STYLUS_RESET:
            isValid = (paraType.Equals("int") && (paraCount == 1));
            break;
        case CommandToServer.SKETCHPAGE_CREATE:
            isValid = (paraType.Equals("int") && (paraCount == 2));
            break;
        case CommandToServer.AVATAR_SYNC:
        case CommandToServer.AVATAR_LEAVE:
            isValid = (paraType.Equals("string") && (paraCount == 2));
            break;
        case CommandToServer.AVATAR_LEAVE_REMOVE_ID:
            isValid = (paraType.Equals("int") && (paraCount == 1));
            break;
        case CommandToServer.SKETCHPAGE_SET:
            isValid = (paraType.Equals("int") && (paraCount == 1));
            break;
        case CommandToServer.MOVE_FW_BW_CTOBJECT:
            isValid = (paraType.Equals("int") && (paraCount == 3));
            break;
        case CommandToServer.INIT_COMBINE:
            break;
        case CommandToServer.UPDATE_STYLUS_Z:
            Debug.LogError("no examples");
            break;
        case CommandToServer.SELECT_CTOBJECT:
            isValid = (paraType.Equals("int") && (paraCount > 0 && paraCount <= 2));
            break;
        case CommandToServer.DESELECT_CTOBJECT:
            isValid = (paraType.Equals("int") && (paraCount == 3));
            break;
        default:
            break;
        }
        //print("Validation: " + isValid + "\t" + cmd);

        return isValid;
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        //base.Update();

    }

    public void ResetBuffer()
    {
        if (host) {
            //Debug.Log("Reset the buffer ");
            host = false;
            curCmdCount = 0;
        }
    }

    //private void FixedUpdate()
    //{        
    //    if (host) {
    //        Debug.Log("Fixedupdate end of frame: " + Time.frameCount);
    //        host = false;
    //        curCmdCount = 0;
    //    }        
    //}

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, 4, false
        );
        curCmdCount = 0;
        byte[] bnCmd = BitConverter.GetBytes(curCmdCount);
        System.Buffer.BlockCopy(bnCmd, 0, data.bytes, 0, bnCmd.Length);
    }

    void OnDestroy()
    {
        print("msgsender bye bye");

        //if (Host)
            //Add(CommandToServer.AVATAR_LEAVE, GlobalToggleIns.GetInstance().username, "0");//msgSender.Add(3, curusername, myAvatar.oculusUserID);

        //base.OnDestroy();
    }
}

class MSGSenderIns {
    public MSGSender sender;

    static public MSGSenderIns ins;

    static public MSGSenderIns GetIns()
    {
        if (ins == null) {
            ins = new MSGSenderIns();
        }
        return ins;
    }

    MSGSenderIns()
    {
        sender = GameObject.Find("Display").GetComponent<MSGSender>();
    }
}
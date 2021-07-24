using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public class MSGReceiver : Holojam.Tools.SynchronizableTrackable {
    [SerializeField] string label = "MSGRcv";
    [SerializeField] string scope = "";

    [SerializeField] bool host = false;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    public string receivedMsg;

    GameObject localAvatar;
    GameObject ctRenderer;
    OculusInput ocInput;
    float[] timestamps = new float[9];
    StylusSyncTrackable stylusSync;
    TimerRecorder timeRecorder;

    // Override Sync()
    protected override void Sync()
    {
        //print("Tracked:" + Tracked);
        if (Tracked) {
            //receivedMsg = Encoding.Default.GetString(data.bytes);
            // receiving several messages
            decode();
            // reset the sender once receive a reply from label MSGRcv
            if (label.Equals("MSGRcv")) {
                MSGSenderIns.GetIns().sender.ResetBuffer();
            }                
        }
    }

    // #0 for resolution
    // #1 for reset ownership
    // #2 for creating sketchPage
    // #3 for receiving avatar name
    void decode()
    {
        int cursor = 0;
        int cmdCount = BitConverter.ToInt16(data.bytes, cursor);
        cursor += 2;
        //print(label + "\tcommand count:" + cmdCount);
        //Utility.Log(0, Color.gray, "decode MSGRcv", "command count:\t" + cmdCount);
        for (int i = 0; i < cmdCount; i++) {
            int cmdNumber = BitConverter.ToInt16(data.bytes, cursor);
            cursor += 2;
            //print("command number:" + cmdNumber);
            switch ((CommandFromServer)cmdNumber) {
            case CommandFromServer.RESOLUTION_REQUEST: {
                // resolution
                Vector2Int res = ParseDisplayInfo(data.bytes, cursor);
                cursor += 4;
                GlobalToggleIns.GetInstance().ChalktalkRes = res;
                break;
            }
            case CommandFromServer.STYLUS_RESET:
                // receive stylus id
                int stylusID = BitConverter.ToInt16(data.bytes, cursor);
                cursor += 2;
                //print("stylus id:" + stylusID);
                //Utility.Log(0, Color.gray, "decode MSGRcv", "stylus id" + stylusID);
                if (GetComponent<StylusSyncTrackable>().ID != stylusID)
                    {
                        GetComponent<StylusSyncTrackable>().SetSend(false);
                        // for active board
                        if(GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree)
                        {
                            ChalktalkBoard.activeBoardID = -1;
                        }
                    }
                    
                break;
            case CommandFromServer.SKETCHPAGE_CREATE:
                // receive page id
                int id = ParseSketchpageID(data.bytes, cursor);
                cursor += 2;

                int setImmediately = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                if (setImmediately == 1) {
                    //Debug.Log("setting board immediately");
                    //Utility.Log(0, Color.gray, "decode MSGRcv", "setting board immediately");
                }
                Utility.Log(0, Color.gray, "decode MSGRcv", "setting board immediately with id " + id);
                //                Debug.Log("received id:" + id + "set immediately?:" + setImmediately);
                //ChalktalkBoard.UpdateCurrentLocalBoard(id);

                break;
            case CommandFromServer.AVATAR_SYNC:
                // add to remote labels if it is not the local one
                //print("add to remote labels");
                if (localAvatar == null)
                    localAvatar = GameObject.Find("LocalAvatar");
                OculusManager om = localAvatar.GetComponent<OculusManager>();

                // receive the whole avatar id mapping.
                int nPair = BitConverter.ToInt16(data.bytes, cursor);
                cursor += 2;
                for (int j = 0; j < nPair; j++) {
                    int nStr = BitConverter.ToInt16(data.bytes, cursor);
                    cursor += 2;
                    string name = Encoding.UTF8.GetString(data.bytes, cursor, nStr);
                    cursor += nStr;
                    //Debug.Log("receive avatar:" + nStr + "\t" + name);
                    Utility.Log(1, Color.yellow, "decode MSGRcv", "receive avatar:" + name);

                    UInt64 remoteID = BitConverter.ToUInt64(data.bytes, cursor);
                    Debug.Log("receive avatar:" + nStr + "\t" + name + "\t" + remoteID);
                    cursor += 8;
                    om.AddRemoteAvatarname(name, remoteID);
                }
                // receive assigned stylus id
                int nAssignedName = BitConverter.ToInt16(data.bytes, cursor);
                cursor += 2;
                string assignedName = Encoding.UTF8.GetString(data.bytes, cursor, nAssignedName);
                cursor += nAssignedName;
                int assignedID = BitConverter.ToInt16(data.bytes, cursor);
                if (assignedName == GlobalToggleIns.GetInstance().username)
                    stylusSync.ID = assignedID;
                cursor += 2;
                break;
            case CommandFromServer.SKETCHPAGE_SET: {
                int boardIndex = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                //Debug.Log("setting page index: " + boardIndex);
                //Utility.Log(1, Color.yellow, "decode MSGRcv", "set page index:" + boardIndex);
                //ChalktalkBoard.UpdateCurrentLocalBoard(boardIndex);
                if(stylusSync.Host)
                    ChalktalkBoard.UpdateActiveBoard(boardIndex);
                //ChalktalkBoard.selectionWaitingForCompletion = false;
                //Debug.Log("<color=orange>SKETCHPAGE SET UNBLOCK</color>" + Time.frameCount);
                break;
            }
            //case CommandFromServer.INIT_COMBINE: {
            //    Debug.Log("initialization data arrived");
            //    // resolution
            //    Vector2Int res = ParseDisplayInfo(data.bytes, cursor);
            //    cursor += 4;
            //    GlobalToggleIns.GetInstance().ChalktalkRes = res;
            //    Debug.Log("setting resolution:[" + res.x + ", " + res.y + "]");

            //    // when first joining, get the active page index
            //    int boardIndex = Utility.ParsetoInt16(data.bytes, cursor);
            //    cursor += 2;
            //    Debug.Log("setting page index: " + boardIndex);
            //    ChalktalkBoard.UpdateCurrentLocalBoard(boardIndex);

            //    if (ctRenderer == null) {
            //        ctRenderer = GameObject.Find("ChalktalkHandler");
            //    }
            //    if (ctRenderer == null) {
            //        Debug.LogError("The renderer is missing");
            //    }
            //    ctRenderer.GetComponent<Chalktalk.Renderer>().enabled = true;
            //    break;
            //}
            case CommandFromServer.SELECT_CTOBJECT: {
                int uid = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;

                if (uid != stylusSync.ID) {
                    cursor += (4 + 2);
                    break;
                }

                float timestamp = Utility.ParsetoRealFloat(data.bytes, cursor);
                cursor += 4;
                //Debug.Log("<color=magenta>" + timestamp + "</color>");
                if (timestamp <= timestamps[6]) {
                    //Debug.Log("<color=blue>Old timestamp arrived for cmd 6</color>");
                    cursor += 2;
                    break;
                }
                else {
                    timestamps[6] = timestamp;
                }

                int status = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                //Debug.Log("<color=green>turn on selection mode, value=[" + status + "]</color>");
                Utility.Log(1, Color.green, "decode MSGRcv", "turn on selection:" + status);
                if (status == 0) {
                    ChalktalkBoard.selectionIsOn = false;
                    //Debug.Log("<color=orange>something was not selected</color>");
                    Utility.Log(1, new Color(1, 165.0f / 255.0f, 0), "decode MSGRcv", "not selected");
                }
                else {
                    //Debug.Log("<color=green>something was selected</color>");
                    Utility.Log(1, Color.green, "decode MSGRcv", "selected");

                    ChalktalkBoard.selectionIsOn = true;
                }

                ChalktalkBoard.selectionWaitingForPermissionToAct = false;
                //Debug.Log("<color=orange>MOVE ON UNBLOCK</color>" + Time.frameCount);

                break;
            }
            case CommandFromServer.DESELECT_CTOBJECT: {
                int uid = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;

                if (uid != stylusSync.ID) {
                    cursor += (4 + 2);
                    break;
                }

                //Debug.Log("<color=green>turn off selection mode</color>");
                Utility.Log(1, Color.green, "decode MSGRcv", "turn off selection");
                float timestamp = Utility.ParsetoRealFloat(data.bytes, cursor);
                cursor += 4;
                if (timestamp <= timestamps[7]) {
                    //Debug.Log("<color=blue>Old timestamp arrived for cmd 7</color>");
                    cursor += 2;
                    break;
                }
                else {
                    timestamps[7] = timestamp;
                }

                //Debug.Log("<color=magenta>" + timestamp + "</color>");

                int status = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                switch (status) {
                case 0:
                    //Debug.Log("NOTHING MOVED");
                    Utility.Log(1, Color.magenta, "decode MSGRcv", "nothing moved");
                    break;
                case 1:
                    //Debug.Log("SKETCH MOVED");
                    Utility.Log(1, Color.magenta, "decode MSGRcv", "sketch moved");
                    break;
                case 2:
                    //Debug.Log("GROUP MOVED");
                    Utility.Log(1, Color.magenta, "decode MSGRcv", "group moved");
                    break;
                }

                ChalktalkBoard.selectionIsOn = false;
                ChalktalkBoard.selectionWaitingForPermissionToAct = false;
                //Debug.Log("<color=orange>MOVE OFF UNBLOCK</color>" + Time.frameCount);
                break;
            }
            case CommandFromServer.AVATAR_LEAVE:
                int nStr2 = BitConverter.ToInt16(data.bytes, cursor);
                cursor += 2;
                string name2 = Encoding.UTF8.GetString(data.bytes, cursor, nStr2);
                cursor += nStr2;
                //print(name2 + "\tis leaving");
                Utility.Log(1, Color.yellow, "decode MSGRcv", name2 + "\tis leaving");
                if (localAvatar != null) {

                    OculusManager om2 = localAvatar.GetComponent<OculusManager>();
                    om2.RemoveRemoteAvatarname(name2);
                    if (name2.Equals(GlobalToggleIns.GetInstance().username)) {
                        //Debug.Log("Calling Application.Quit()");
                        // write down
                        timeRecorder.finalize();
                        Application.Quit();
                    }
                }
                else {
                    //Debug.Log("LocalAvatar is null");
                    Utility.Log(2, Color.red, "decode MSGRcv", "LocalAvatar is null");
                }

                break;
            case CommandFromServer.UPDATE_STYLUS_Z: {
                float timestamp = Utility.ParsetoRealFloat(data.bytes, cursor);
                cursor += 4;
                if (timestamp <= timestamps[8]) {
                    cursor += 4;
                    break;
                }
                else {
                    timestamps[8] = timestamp;
                }

                float zOffset = Utility.ParsetoRealFloat(data.bytes, cursor);
                cursor += 4;

                //Debug.Log("<color=red>z-offset" + zOffset + "</color>");
                Utility.Log(1, Color.red, "decode MSGRcv", "zOffset:\t" + zOffset);
                stylusSync.zOffset = zOffset;

                break;
            }
            case CommandFromServer.SELECTION_RESET: {

                // (KTR) reset all selections here (and anything else necessary)

                Debug.Log("Resetting selections");

                ChalktalkBoard.selectionIsOn = false;
                ChalktalkBoard.selectionWaitingForPermissionToAct = false;

                if (ocInput == null) {
                    ocInput = GameObject.Find("oculusController").GetComponent<OculusInput>();
                }
                ocInput.controlInProgress = false;
                ocInput.depthPositionControlInProgress = false;

                break;
            }
            case CommandFromServer.COUNT_DOWN:
                int duration = BitConverter.ToInt16(data.bytes, cursor);
                cursor += 2;
                Utility.Log(0, Utility.logSuccess, "MSG RCV", "count down: " + duration);
                timeRecorder.StartCountDown(duration);
                break;
            default:
                break;
            }
        }

    }
    int ParseSketchpageID(byte[] bytes, int offset = 0)
    {

        if (bytes.Length >= offset) {
            int cursor = offset;
            int cnt = Utility.ParsetoInt16(bytes, cursor);
            return cnt;
        }
        return 0;
    }
    Vector2Int ParseDisplayInfo(byte[] bytes, int offset = 0)
    {
        if (bytes.Length >= offset) {
            int cursor = offset;
            int resW = Utility.ParsetoInt16(bytes, cursor);
            cursor += 2;
            int resH = Utility.ParsetoInt16(bytes, cursor);
            return new Vector2Int(resW, resH);
        }
        return new Vector2Int(0, 0);
    }

    int ParseSketchpageCnt(byte[] bytes, int offset = 0)
    {
        if (bytes.Length > 8) {
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

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, 0, false
        );
        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        timeRecorder = GameObject.Find("Timer").GetComponent<TimerRecorder>();
    }
}

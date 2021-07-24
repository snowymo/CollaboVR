using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KeyboardInput : MonoBehaviour
{

    GameObject ChalktalkHandler;
    Chalktalk.Renderer ctRenderer;
    CreateSPSync createSP;
    //MSGSender msgSender;
    PerspectiveView perspView;
    LineRenderer lr;

    StylusSyncTrackable stylusSync;

    // Use this for initialization
    void Start()
    {
        ChalktalkHandler = GameObject.Find("ChalktalkHandler");
        ctRenderer = ChalktalkHandler.GetComponent<Chalktalk.Renderer>();
        //msgSender.Send(0, new int[] { });
        
        perspView = GameObject.Find("LocalAvatar").GetComponent<PerspectiveView>();
        lr = GetComponent<LineRenderer>();
        Vector3[] playArea = new Vector3[4];
        if (OVRManager.boundary != null && OVRManager.boundary.GetConfigured())
            playArea = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
        
        lr.SetPositions(playArea);
        lr.SetPosition(4, playArea[0]);
        //lr.enabled = false;
    }

    // Update is called once per frame
    static int toggle = 0;


    void Update()
    {
        if(GlobalToggleIns.GetInstance().chalktalkRes.x == 0)
            MSGSenderIns.GetIns().sender.Add((int)CommandToServer.RESOLUTION_REQUEST, new int[] { });
        if (Input.GetKeyDown(KeyCode.A)) {
            // test all cases
            MSGSenderIns.GetIns().sender.Add(CommandToServer.STYLUS_RESET, new int[] { 0 });
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SKETCHPAGE_CREATE, new int[] { ChalktalkBoard.curMaxBoardID, 0 });
            MSGSenderIns.GetIns().sender.Add(CommandToServer.AVATAR_SYNC, "test", "0");
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SKETCHPAGE_SET, new int[] { 0 });
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SELECT_CTOBJECT, new int[] { Time.frameCount, 0 });
            MSGSenderIns.GetIns().sender.Add(CommandToServer.DESELECT_CTOBJECT, new int[] { Time.frameCount, 0, 0 });
            MSGSenderIns.GetIns().sender.Add(CommandToServer.MOVE_FW_BW_CTOBJECT, new int[] { Time.frameCount, 1, 0});
            // use for testing
            //ctRenderer.CreateBoard();
            // add a new page
            //msgSender.Add((int)CommandToServer.SKETCHPAGE_CREATE, new int[] { ChalktalkBoard.curMaxBoardID, 1 });
            // ask for resolution
            //msgSender.Add((int)CommandToServer.RESOLUTION_REQUEST, new int[] { });
            // switch btw pages
            //msgSender.Add((int)CommandToServer.SKETCHPAGE_SET, new int[] { ChalktalkBoard.currentBoardID + 1 });
            //msgSender.Add((int)CommandToServer.SKETCHPAGE_SET, new int[] { Utility.Mod(ChalktalkBoard.currentBoardID - 1, ChalktalkBoard.MaxExistingID()) });
            // test ownership
            //msgSender.Add(1, new int[] { 0 });
            // init
            //msgSender.Add((int)CommandToServer.INIT_COMBINE, new int[] { });
            //print("sending test:\t" + ctRenderer.ctBoards.Count);
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            //msgSender.Send("test");
            //msgSender.Send(0, new int[] { });
            //StylusSyncTrackable stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
            //msgSender.Send(1, new int[] { stylusSync.ID });
            //msgSender.Add((int)CommandToServer.SKETCHPAGE_CREATE, new int[] { ChalktalkBoard.curMaxBoardID});
            //print("sending test:\t" + ctRenderer.ctBoards.Count);
            MSGSenderIns.GetIns().sender.Add(CommandToServer.AVATAR_LEAVE, GlobalToggleIns.GetInstance().username, "0");//msgSender.Add(3, curusername, myAvatar.oculusUserID);
            if (stylusSync == null)
                stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();

            MSGSenderIns.GetIns().sender.Add(CommandToServer.AVATAR_LEAVE_REMOVE_ID, new int[] { stylusSync.ID });


            //UnityEditor.EditorApplication.Exit(0);
        }
        //if (Input.GetKeyDown(KeyCode.B)) {
        //    switch (ChalktalkBoard.Mode.flags) {
        //        case ChalktalkBoard.ModeFlags.NONE: {
        //            msgSender.Send(6, new int[] { });
        //            break;
        //        }
        //        case ChalktalkBoard.ModeFlags.TEMPORARY_BOARD_ON: {
        //            msgSender.Send(7, new int[] { });
        //            break;
        //        }
        //        case ChalktalkBoard.ModeFlags.TEMPORARY_BOARD_TURNING_OFF: {
        //            ChalktalkBoard.Mode.flags = ChalktalkBoard.ModeFlags.NONE;
        //            break;
        //        }
        //    }

        //    Debug.Log("sending test board transferring message");
        //}
        if (Input.GetKeyDown(KeyCode.B)) {
            // temporarily just moves the currently selected sketch to the next board
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SELECT_CTOBJECT, new int[] { 0 });
        }

        if (Input.GetKeyDown(KeyCode.Minus)) {
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SKETCHPAGE_SET, new int[] { Utility.Mod(ChalktalkBoard.currentLocalBoardID + 1, 4) });
        }
        if (Input.GetKeyDown(KeyCode.Equals)) {
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SKETCHPAGE_SET, new int[] { Utility.Mod(ChalktalkBoard.currentLocalBoardID - 1, 4) });
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            // toggle
            //toggle = 1 - toggle;
            //MSGSenderIns.GetIns().sender.Add((int)CommandToServer.INIT_COMBINE, new int[] { toggle, 562000 });
            perspView.DoObserve(2);
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            lr.enabled = !lr.enabled;
            if (lr.enabled) {
                Vector3[] playArea = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
                lr.SetPositions(playArea);
                lr.SetPosition(4, playArea[0]);
            }            
        }
    }
}

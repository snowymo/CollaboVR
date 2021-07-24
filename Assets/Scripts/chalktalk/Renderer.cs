using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Chalktalk {
    // retrieve label display from display object under holojam
    public class Renderer : MonoBehaviour {
        // container for display data
        byte[] displayData;

        // labels
        DisplaySyncTrackable displaySync;

        MeshDisplaySyncTrackable displaySyncMesh;

        // world
        GameObject world;

        // prefab for each line in chalktalk sketch
        public SketchCurve sketchLine; // or we can just create a new instance when we want to if it is not a gameObject

        // container for chalktalk board and chalktalk line
        //[SerializeField]
        //public List<ChalktalkBoard> ctboard; // multiple chalktalk boards, support runtime creation
        List<SketchCurve> ctSketchLines;
        public ChalktalkBoard ctBoardPrefab;

        // parser
        ChalktalkParse ctParser;

        // pool support
        public CTEntityPool entityPool = new CTEntityPool();
        public int initialLineCap = 0;
        public int initialFillCap = 0;
        public int initialTextCap = 0;

        // vive calibration
        LHOwnSync ownLightHouse;
        LHRefSync refLightHouse;

        // for resolution
        MSGSender msgSender;
        float prevGlobalToggleBoardScale = 0, prevGTHorizontalScale = 0;
        Vector3 prevGlobalShift;
        float prevDisToCenter;
        bool initCTPrefab = false;
        private void Awake()
        {

        }

        public void UpdateCTBoardPrefab()
        {
            // once we have resolution, we need to update the local scale of the prefab
            float newx = 2 * GlobalToggleIns.GetInstance().ChalktalkBoardScale;
            float newy = newx / (GlobalToggleIns.GetInstance().ChalktalkRes.x / GlobalToggleIns.GetInstance().ChalktalkRes.y);
            //ctBoardPrefab.bc = ctBoardPrefab.transform.Find("collider").GetComponent<BoxCollider>();
            ctBoardPrefab.bc.transform.localScale = new Vector3(newx, newy, 1f);
            prevGlobalShift = GlobalToggleIns.GetInstance().globalShift;
            prevDisToCenter = GlobalToggleIns.GetInstance().disToCenter;
            initCTPrefab = true;
        }

        // Use this for initialization
        void Start()
        {
            //msgSender.Add((int)CommandToServer.INIT_COMBINE, new int[] { });

            //Debug.Log("starting");
            world = GameObject.Find("World");

            //ChalktalkBoard.boardList = ctBoards;

            //CreateBoard();

            GameObject display = GameObject.Find("Display");
            displaySync = display.GetComponent<DisplaySyncTrackable>();

            displaySyncMesh = display.GetComponent<MeshDisplaySyncTrackable>();

            ownLightHouse = display.GetComponent<LHOwnSync>();
            refLightHouse = display.GetComponent<LHRefSync>();

            ctSketchLines = new List<SketchCurve>();
            if (GlobalToggleIns.GetInstance().poolForSketch == GlobalToggle.PoolOption.Pooled) {
                entityPool.Init(
                    sketchLine.gameObject, sketchLine.gameObject, sketchLine.gameObject,
                    initialLineCap, initialFillCap, initialTextCap
                );
            }
            //displayData = new byte[0];
            ctParser = new ChalktalkParse();
        }

        // Update is called once per frame
        void Update()
        {
            // update board res
            if (GlobalToggleIns.GetInstance().chalktalkRes.x != 0) {
                if(!initCTPrefab)
                    UpdateCTBoardPrefab();
            }
            else
                return;

            if (prevGlobalToggleBoardScale != GlobalToggleIns.GetInstance().ChalktalkBoardScale) {
                UpdateCTBoardScale();
                UpdateCTBoardPos();
            }

            if(prevGTHorizontalScale != GlobalToggleIns.GetInstance().horizontalScale) {
                UpdateCTHorizontalScale();
                UpdateCTBoardPos();
            }

            if((prevGlobalShift != GlobalToggleIns.GetInstance().globalShift)
                || (prevDisToCenter != GlobalToggleIns.GetInstance().disToCenter)) {
                UpdateCTBoardPos();
            }

            // update all boards' transform
            if (ownLightHouse.Tracked && refLightHouse.Tracked) {
                Matrix4x4 mOwn = Matrix4x4.TRS(ownLightHouse.Pos, ownLightHouse.Rot, Vector3.one);
                Matrix4x4 mRef = Matrix4x4.TRS(refLightHouse.Pos, refLightHouse.Rot, Vector3.one);
                foreach (ChalktalkBoard b in ChalktalkBoard.boardList) {
                    Vector3 p = b.transform.position;
                    // TODO: p should be the original place in source's coordinate system
                    // which is 0,1,0 for now
                    p = new Vector3(0, 1f, 0);
                    Vector4 p4 = mOwn * mRef.inverse * new Vector4(p.x, p.y, p.z, 1);
                    b.transform.position = new Vector3(p4.x, p4.y, p4.z);

                    Matrix4x4 mq = Matrix4x4.Rotate(Quaternion.identity);
                    b.transform.rotation = (mOwn * mRef.inverse * mq).rotation;
                }
            }

            //
            if (displaySync.Tracked && displaySync.publicData != null && displaySync.publicData.Length > 0) {
                // retrieve and parse the data
                ctSketchLines.Clear();
                ctParser.Parse(displaySync.publicData, ref ctSketchLines, ref entityPool);
                // apply the transformation from the specific board to corresponding data
                while (!entityPool.ApplyBoard(ChalktalkBoard.boardList))
                    CreateBoard(new Vector3(1.5f, 0, 1.5f), Quaternion.Euler(0, 90, 0));
                //foreach (SketchCurve sc in ctSketchLines)
                //sc.ApplyTransform(ctBoards);
                // draw them
                entityPool.FinalizeFrameData();
                // Draw()
            }

            if (displaySyncMesh.Tracked && displaySyncMesh.publicData != null && displaySyncMesh.publicData.Length > 0) {
                ctParser.ParseMesh(displaySyncMesh.publicData);
            }
        }

        void UpdateCTBoardScale()
        {
            prevGlobalToggleBoardScale = GlobalToggleIns.GetInstance().ChalktalkBoardScale;
            float newx = 2 * prevGlobalToggleBoardScale;
            float newy = newx / (GlobalToggleIns.GetInstance().ChalktalkRes.x / GlobalToggleIns.GetInstance().ChalktalkRes.y);
            for (int i = 0; i < ChalktalkBoard.boardList.Count; i++) {
                if (GlobalToggleIns.GetInstance().MRConfig != GlobalToggle.Configuration.eyesfree
                    || i != 0)
                    ChalktalkBoard.boardList[i].bc.transform.localScale = new Vector3(newx, newy, 1f);
            }
        }

        void UpdateCTHorizontalScale()
        {
            prevGTHorizontalScale = GlobalToggleIns.GetInstance().horizontalScale;
            if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
                float newx = 2 * prevGlobalToggleBoardScale * prevGTHorizontalScale;
                float newy = newx / (GlobalToggleIns.GetInstance().ChalktalkRes.x / GlobalToggleIns.GetInstance().ChalktalkRes.y);
                if (ChalktalkBoard.boardList.Count > 0) {
                    ChalktalkBoard.boardList[0].bc.transform.localScale = new Vector3(newx, newy, 1f);
                }
            }
            //ChalktalkBoard.UpdateCurrentLocalBoard(ChalktalkBoard.currentLocalBoardID);
        }

        void UpdateCTBoardPos()
        {
            if (ChalktalkBoard.boardList.Count == 0)
                return;

            for (int i = 0; i < ChalktalkBoard.boardList.Count; i++) {
                if (GlobalToggleIns.GetInstance().MRConfig != GlobalToggle.Configuration.eyesfree
                    || i != 0)
                    ChalktalkBoard.CalculatePosRotBasedOnID(
                    ChalktalkBoard.boardList[i].bc.transform.localScale, ChalktalkBoard.boardList[i]);
            }
            if(ChalktalkBoard.currentLocalBoardID >= 0) {
                ChalktalkBoard.UpdateCurrentLocalBoard(ChalktalkBoard.currentLocalBoardID);
            }            
            prevGlobalShift = GlobalToggleIns.GetInstance().globalShift;
            prevDisToCenter = GlobalToggleIns.GetInstance().disToCenter;
        }

        StringBuilder sbDebug = new StringBuilder();
        public string meshMessage;
        public void CreateBoard(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
        {
            // create the board based on the configuration
            switch (GlobalToggleIns.GetInstance().MRConfig) {
            case GlobalToggle.Configuration.sidebyside: {
                ChalktalkBoard.CreateOrUpdateBoard(ctBoardPrefab, world.transform);
                ChalktalkBoard.boardList[ChalktalkBoard.curBoardIndex - 1].transform.localScale = Vector3.one;
            }
            break;
            case GlobalToggle.Configuration.mirror:
                //if(ctBoards.Count == 0)
                {
                ChalktalkBoard.CreateOrUpdateBoard(ctBoardPrefab, world.transform);
                ChalktalkBoard.boardList[ChalktalkBoard.curBoardIndex - 1].transform.localScale = Vector3.one;
            }
            break;
            case GlobalToggle.Configuration.eyesfree:
                if (ChalktalkBoard.curMaxBoardID == 0) {
                    // change scale
                    Vector3 prevOne = ctBoardPrefab.bc.transform.localScale;
                    ctBoardPrefab.bc.transform.localScale = new Vector3(ctBoardPrefab.bc.transform.localScale.x * GlobalToggleIns.GetInstance().horizontalScale,
                        ctBoardPrefab.bc.transform.localScale.y * GlobalToggleIns.GetInstance().horizontalScale,
                        ctBoardPrefab.bc.transform.localScale.z);
                    ChalktalkBoard.CreateOrUpdateBoard(ctBoardPrefab, world.transform);
                    ChalktalkBoard.boardList[ChalktalkBoard.curBoardIndex - 1].transform.localScale = new Vector3(1f, 1f, 0.001f);
                    ctBoardPrefab.bc.transform.localScale = prevOne;
                    ChalktalkBoard.CreateOrUpdateBoard(ctBoardPrefab, world.transform, "Dup", -1);
                    ChalktalkBoard.boardList[ChalktalkBoard.curBoardIndex - 1].transform.localScale = Vector3.one;
                }
                else {
                    ChalktalkBoard.CreateOrUpdateBoard(ctBoardPrefab, world.transform, "Dup");
                    ChalktalkBoard.boardList[ChalktalkBoard.curBoardIndex - 1].transform.localScale = Vector3.one;
                }
                break;
            default:
                break;
            }
            ChalktalkBoard.UpdateCurrentLocalBoard(ChalktalkBoard.currentLocalBoardID);
        }

        //void Draw()
        //{
        //    for (int i = 0; i < ctSketchLines.Count; i += 1)
        //    {
        //        ctSketchLines[i].Draw();
        //    }
        //}
    }

}

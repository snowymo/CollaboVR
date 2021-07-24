using System.Collections.Generic;
using UnityEngine;

public class ChalktalkBoard : MonoBehaviour {

    public static int currentLocalBoardID = 0; // the board id of the current user, locally
		public static int activeBoardID = -1;	// the active board from chalktalk server side, universally
    public static int curMaxBoardID = 0;//next board id to create
    public int boardID;// the same as sketchPageID
    public static int curBoardIndex = 0; // the same to curmaxBoardID except for eyesfree, at that time curBoardIndex = curMaxBoardID +1
    static List<ChalktalkBoard> returnBoardList = new List<ChalktalkBoard>();

    public static List<ChalktalkBoard> boardList = new List<ChalktalkBoard>();
    // TODO support duplicates as a list so IDs match with the boards
    public static List<List<ChalktalkBoard>> duplicateBoardList = new List<List<ChalktalkBoard>>(); 

    public static bool selectionIsOn = false;
    public static bool selectionWaitingForPermissionToAct = false;

    public static int latestUpdateFrame = 0;
    public BoxCollider bc;

    public float boardScale { get {
            return bc != null ? bc.transform.localScale.x / 2.0f : GlobalToggleIns.GetInstance().ChalktalkBoardScale;
        }
    }

    // Use this for initialization
    void Start()
    {
        //returnBoardList = new List<ChalktalkBoard>();
        bc = transform.Find("collider").GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(bc == null) {
        //    bc = transform.Find("collider").GetComponent<BoxCollider>();
        //}
    }

    public static void Reset()
    {
        currentLocalBoardID = 0;
        activeBoardID = -1;
        curMaxBoardID = 0;
        curBoardIndex = 0;
        // clean up eyes free helper
        ResetEyesfreeHelper();
        //for(int i = 0; i < boardList.Count; i++) {
        //boardList[i].enabled = false;
        //Destroy(boardList[i].gameObject);
        //}
        //boardList.Clear();
        selectionIsOn = false;
        selectionWaitingForPermissionToAct = false;
    }

    public static void CalculatePosRotBasedOnID(Vector3 prefabScale, ChalktalkBoard ctBoard)
    {
        Vector3 boardPos = new Vector3(ctBoard.boardID / 4 + GlobalToggleIns.GetInstance().disToCenter, 0, 0);
        boardPos = Quaternion.Euler(0, (ctBoard.boardID) * -90 + (ctBoard.boardID) / 4 * 45, 0) * boardPos;
        //boardPos.z += 2;
        ctBoard.transform.localPosition = boardPos + GlobalToggleIns.GetInstance().globalShift;
        ctBoard.transform.localRotation = Quaternion.Euler(0, 90 + (-ctBoard.boardID) * 90 + (-ctBoard.boardID) / 4 * 45, 0);
        ctBoard.bc.transform.localScale = prefabScale;
    }

    public static void CreateOrUpdateBoard(ChalktalkBoard ctBoardPrefab, Transform world, string suffix = "", int idAdjust = 0)
    {
        ChalktalkBoard ctBoard = null;
        if (curBoardIndex < boardList.Count) {
            // update the board
            ctBoard = boardList[curBoardIndex];
        }
        else if(curBoardIndex == boardList.Count){
            // create the board
            ctBoard = Instantiate(ctBoardPrefab, world) as ChalktalkBoard;
            boardList.Add(ctBoard);
        }
        ctBoard.boardID = curMaxBoardID + idAdjust;
        ctBoard.name = "Board" + ctBoard.boardID.ToString() + suffix;
        //
        CalculatePosRotBasedOnID(ctBoardPrefab.bc.transform.localScale, ctBoard);
        //Vector3 boardPos = new Vector3(ctBoard.boardID / 4 + GlobalToggleIns.GetInstance().disToCenter, 0, 0);
        //boardPos = Quaternion.Euler(0, (ctBoard.boardID) * -90 + (ctBoard.boardID) / 4 * 45, 0) * boardPos;
        
        //ctBoard.transform.localPosition = boardPos + GlobalToggleIns.GetInstance().globalShift;
        //ctBoard.transform.localRotation = Quaternion.Euler(0, 90 + (-ctBoard.boardID) * 90 + (-ctBoard.boardID) / 4 * 45, 0);
        //ctBoard.bc.transform.localScale = ctBoardPrefab.bc.transform.localScale;
        //ctBoard.gameObject.transform.localScale *= GlobalToggleIns.GetInstance().ChalktalkBoardScale;

        //boardList[curBoardIndex] = ctBoard;
        curMaxBoardID = ctBoard.boardID + 1;
        ++curBoardIndex;
    }

    public static ChalktalkBoard GetCurLocalBoard() {
        if (currentLocalBoardID < 0 || boardList.Count == 0)
            return null;
        //return boardList[currentBoardID];
        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            if(boardList.Count > currentLocalBoardID+1)
                return boardList[currentLocalBoardID + 1];
            else
                return boardList[currentLocalBoardID];
        }            
        else
            return boardList[currentLocalBoardID];
    }

    public static List<ChalktalkBoard> GetBoard(int index)
    {
        returnBoardList.Clear();
        if (index >= curMaxBoardID)
            return returnBoardList;

        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            returnBoardList.Add(boardList[index + 1]);
            if (index == activeBoardID)
                returnBoardList.Add(boardList[0]);
        }
        else
            returnBoardList.Add(boardList[index]);
        return returnBoardList;
    }

    public static bool isOutlineOfFrame(Vector3[] points)
    {
        bool ret = true;
        if (points.Length == 5) {
            for (int i = 0; i < points.Length; i++) {
                if ((points[i].x >= 1.0f) && (points[i].x <= -1.0f)) {
                    ret = false;
                    break;
                }
            }
            return ret;
        }
        else
            return false;
    }

    public static void UpdateCurrentLocalBoard(int id)
    {
        currentLocalBoardID = id;
        //if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
        //    UpdateActivetBoardEyesfree(currentLocalBoardID);
        //}        
    }

	public static void UpdateActiveBoard(int id) {
		activeBoardID = id;
        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            UpdateActivetBoardEyesfree(activeBoardID);
        }
    }

    static void UpdateActivetBoardEyesfree(int id)
    {
        // change whenever current board changes
        boardList[0].boardID = id;
        boardList[0].name = "Board" + id.ToString();
        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
        

        boardList[0].transform.localRotation = GetCurLocalBoard().transform.rotation * Quaternion.Euler(90f, 0, 0);

        float newx = 2 * GlobalToggleIns.GetInstance().ChalktalkBoardScale * GlobalToggleIns.GetInstance().horizontalScale;
        float newy = newx / (GlobalToggleIns.GetInstance().ChalktalkRes.x / GlobalToggleIns.GetInstance().ChalktalkRes.y);

        boardList[0].bc.transform.localScale = new Vector3(newx,
                        newy,
                        1f);
        Vector3 p = GetCurLocalBoard().transform.TransformPoint(
            0,
            -0.5f * GlobalToggleIns.GetInstance().ChalktalkBoardScale - 0.1f/*collider size*/,
            +newy - GlobalToggleIns.GetInstance().disToCenter/ GlobalToggleIns.GetInstance().ChalktalkBoardScale);//
        //boardList[0].transform.localPosition = new Vector3(p.x/boardList[0].bc.transform.localScale.x, 
            //p.y/ boardList[0].bc.transform.localScale.y, 
            //p.z/ boardList[0].bc.transform.localScale.z);
        boardList[0].transform.position = p;
        // disable all helpers
        EyesfreeHelper helper = null;
        for (int i = 0; i < boardList.Count-1; i++) {
            helper = boardList[i + 1].gameObject.GetComponent<EyesfreeHelper>();
            if (helper != null) {
                helper.isFocus = false;
            }
        }        
        
        // update cursor
        helper = boardList[id + 1].gameObject.GetComponent<EyesfreeHelper>();
        if(helper == null) {
            helper = boardList[id + 1].gameObject.AddComponent<EyesfreeHelper>();
            
            helper.activeBindingbox = boardList[0].transform;
            helper.activeCursor = GameObject.Find("cursor").transform;
            helper.dupCursor = GameObject.Find("dupcursor").transform;
            helper.dupCursor.Find("Cube").GetComponent<MeshRenderer>().enabled = true;
        }            
        helper.dupBindingbox = boardList[id + 1].gameObject.transform;
        helper.isFocus = true;
    }

    static void ResetEyesfreeHelper()
    {
        for(int i = 0; i < boardList.Count; i++) {
            EyesfreeHelper helper = boardList[i].gameObject.GetComponent<EyesfreeHelper>();
            if(helper != null) {
                helper.dupCursor.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                helper.isFocus = false;
                Destroy(helper);
            }             
        }
    }
}

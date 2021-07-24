using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineQuad : MonoBehaviour {
    public GameObject world; //parent of boards
    private int _boardID; //keeps track of which board the outline is currently placed at
    private ChalktalkBoard _boardObj;
    private bool _assignFirstBoard = false; //makes sure that the outline is assigned after the boards created
    private Renderer rend;

    private GlowComposite glowComposite;
    private GlowController glowController;

    int boardLatestUpdateFrame = 0;
    float prevGlobalToggleBoardScale;
    Vector3 prevGlobalShift;
    float prevDisToCenter;

    private void Start()
    {
        this.rend = GetComponent<Renderer>();

        world = GameObject.Find("World");

        prevGlobalToggleBoardScale = GlobalToggleIns.GetInstance().ChalktalkBoardScale;
        prevGlobalShift = GlobalToggleIns.GetInstance().globalShift;
        prevDisToCenter = GlobalToggleIns.GetInstance().disToCenter;
        //glowComposite = Camera.main.gameObject.AddComponent<GlowComposite>();
        //glowComposite.Intensity = 6.59f;

        //glowController = Camera.main.gameObject.AddComponent<GlowController>();
    }

    // Update is called once per frame
    void Update () {
        if (!_assignFirstBoard) {
            if (SetPositionOrientation()) {
                _boardID = ChalktalkBoard.currentLocalBoardID;
                _boardObj = ChalktalkBoard.GetCurLocalBoard();
                _assignFirstBoard = true;   // zhenyi: it makes no sense to change this flag from false to false. So I revised it.
            }            
        }
        //only update position if a new board was selected or the current board was moved 
        else if ((_boardID != ChalktalkBoard.currentLocalBoardID) || (ChalktalkBoard.latestUpdateFrame > this.boardLatestUpdateFrame)
            || (prevGlobalToggleBoardScale != GlobalToggleIns.GetInstance().ChalktalkBoardScale) 
            || (prevGlobalShift != GlobalToggleIns.GetInstance().globalShift)
            || (prevDisToCenter != GlobalToggleIns.GetInstance().disToCenter)) { 
            if (SetPositionOrientation()) {
                ChalktalkBoard.latestUpdateFrame = this.boardLatestUpdateFrame;
                _boardID = ChalktalkBoard.currentLocalBoardID;
                _boardObj = ChalktalkBoard.GetCurLocalBoard();
                prevGlobalToggleBoardScale = GlobalToggleIns.GetInstance().ChalktalkBoardScale;
                prevGlobalShift = GlobalToggleIns.GetInstance().globalShift;
                prevDisToCenter = GlobalToggleIns.GetInstance().disToCenter;
            }            
        }
	}

    //Summary
    //Set position orientation will find the board in the world that corresponds to the current
    //board id. Then, it will set the glowing outline to the same rotation, position,
    //and size of the currently selected board.
    bool SetPositionOrientation(){
        // we can change GetCurLocalBoard() to activeBoardID if the requirement changes
        ChalktalkBoard ctb = ChalktalkBoard.GetCurLocalBoard();
        if (ctb != null) {
            GameObject boardToOutline = ctb.gameObject;
            gameObject.transform.position = boardToOutline.transform.position;
            gameObject.transform.rotation = boardToOutline.transform.rotation;
            gameObject.transform.localScale = boardToOutline.transform.Find("collider").localScale;
            return true;
        }
        else
            return false;
		
    }
}

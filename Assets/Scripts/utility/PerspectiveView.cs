using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class PerspectiveView : MonoBehaviour {

    private OVRManager ovrManager;
    private OculusManager oculusManager;
    private GameObject OVRCameraRig;

    private OvrAvatar ovrAvatar;
    public string observeeName;
    private SyncUserData observee;

    public bool isObserving;
    Vector3 posBeforeObserve;
    LineRenderer lr;
    VectorLine vectorLine;

    public GameObject RTCameraPrefab;
    Transform RTCamera;
    public GameObject perspPlane;
    MeshRenderer perspPlaneMR;
    public Texture2D lineTex, frontTex, texture;

    bool usingVectrosity = false;
    Vector3[] perspPlanePoses = new Vector3[] { new Vector3(1.26f, -0.96f, 4.2f ),
    new Vector3(-1.26f, -0.96f, 4.2f ),
    new Vector3(-1.26f, 0.96f, 4.2f ),
    new Vector3(1.26f, 0.96f, 4.2f )};
    int perspPlanePosIndex = 0;

    void Start()
    {
        OVRCameraRig = GameObject.Find("OVRCameraRig");
        ovrManager = OVRCameraRig.GetComponent<OVRManager>();
        isObserving = false;

        oculusManager = gameObject.GetComponent<OculusManager>();
        ovrAvatar = gameObject.GetComponent<OvrAvatar>();
        lr = gameObject.GetComponent<LineRenderer>();
        lr.enabled = false;
        vectorLine = new VectorLine("perspRay", new List<Vector3>() { Vector3.zero, Vector3.zero }, 10);
        //vectorLine.color = new Color(255, 165, 0);
        vectorLine.lineType = LineType.Continuous;
        vectorLine.texture = texture;
        observeOffset = new Vector3(0, -0.2f, 0.5f);
        observeeName = "";
        perspPlane = GameObject.Find("perspPlane");
        perspPlaneMR = perspPlane.GetComponent<MeshRenderer>();
        RTCamera = Instantiate(RTCameraPrefab).transform;
        RTCamera.position = Vector3.zero;
        RTCamera.forward = Vector3.forward;
                VectorLine.SetEndCap("a", EndCap.Mirror, lineTex, frontTex);        vectorLine.endCap = "a";        VectorManager.useDraw3D = true;
        vectorLine.Draw3DAuto();        vectorLine.active = false;
    }

    public void MovePerspPlane(bool clockwise)
    {
        if (clockwise) {
            perspPlanePosIndex = Utility.Mod(perspPlanePosIndex + 1, 4);
        }
        else
            perspPlanePosIndex = Utility.Mod(perspPlanePosIndex - 1, 4);
        perspPlane.transform.localPosition = perspPlanePoses[perspPlanePosIndex];
    }

    public void DoObserve(int state, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
    {
        print("tryObserve start: curState " + isObserving);
        if (state == 0)
        {
            // button down
            if (!isObserving)
            {
                SelectObservee(pos, rot);
            }
        }
        else if (state == 1)
        {
            // button up
            if (!isObserving)
            {
                ObserveObservee();
            }
            else
            {
                DisableObserve();
            }
        }
        else if (state == 2)
        {
            // use keycode
            if (!isObserving)
            {
                if (oculusManager.remoteNames.Count > 0)
                {
                    oculusManager.usernameToUserDataMap.TryGetValue(oculusManager.remoteNames[0], out observee);
                    print("Observing:" + oculusManager.remoteNames[0]);
                    ObserveObservee();
                }
            }
            else
            {
                DisableObserve();
            }

        }
        //print("tryObserve end: curState " + isObserving);
    }

    void SelectObservee(Vector3 pos, Quaternion rot)
    {
        // find the observee
        if (oculusManager.remoteAvatars.Count > 0)
        {
            // either use ray cast or 0 by default
            RaycastHit hit;
            int layerMask = 1 << 12;
            //layerMask = ~layerMask;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(pos, rot * Vector3.forward, out hit, Mathf.Infinity, layerMask))
            {
                if (!usingVectrosity) {
                    lr.SetPosition(0, pos);
                    lr.SetPosition(1, pos + rot * Vector3.forward * hit.distance);
                    lr.enabled = true;
                }
                else {
                    vectorLine.points3[0] = pos;
                    vectorLine.points3[1] = pos + rot * Vector3.forward * hit.distance;
                    vectorLine.active = true;
                }
                
                
                //Gizmos.DrawLine(pos, rot * Vector3.forward * hit.distance);
                //Gizmos.color = Color.yellow;
                observeeName = hit.transform.name.Substring(7);//get rid of "remote-"
                                                               // if the observee is observing, then shift to next or just cancel this
                oculusManager.usernameToUserDataMap.TryGetValue(observeeName, out observee);
                print("Observing:" + observeeName);

                if(GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.mirror) {
                    // we need to enable observee without mirroring

                }
            }
            else
            {
                //Gizmos.DrawRay(pos, rot * Vector3.forward);
                //Gizmos.color = Color.red;
                if (!usingVectrosity) {
                    lr.SetPosition(0, pos);
                    lr.SetPosition(1, pos + rot * Vector3.forward * 2);
                    lr.enabled = true;
                }
                else {
                    vectorLine.points3[0] = pos;
                    vectorLine.points3[1] = pos + rot * Vector3.forward * 2;
                    vectorLine.active = true;
                }
                
                observee = null;
                observeeName = "";
            }
        }
    }

    void ObserveObservee()
    {
        if (observee != null)
        {
            //oculusManager.remoteAvatars[0].gameObject.SetActive(false);
            
            if(GlobalToggleIns.GetInstance().perspMode != GlobalToggle.ObserveMode.RT)
            {
                if (!observee.UserIsObserving()) {
                    // turn off position tracking
                    ovrManager.usePositionTracking = false;
                    // turn off thrid view of local avatar
                    ovrAvatar.ShowThirdPerson = false;
                    // turn off packet record?
                    ovrAvatar.RecordPackets = false;
                }                
            }
            else {
                perspPlaneMR.enabled = true;
            }
            
            // record the pos
            posBeforeObserve = OVRCameraRig.transform.position;

            isObserving = true;
            
        }
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, Vector3.zero);
        lr.enabled = false;
        vectorLine.active = false;
    }

    void DisableObserve()
    {
        if (GlobalToggleIns.GetInstance().perspMode != GlobalToggle.ObserveMode.RT)
        {
            // turn on position tracking
            ovrManager.usePositionTracking = true;
            // turn on thrid view of local avatar
            ovrAvatar.ShowThirdPerson = true;
            // turn on packet record?
            ovrAvatar.RecordPackets = true;
        }            
        // reset observee
        //if (oculusManager.remoteAvatars.Count > 0) {
        //    oculusManager.remoteAvatars[0].gameObject.SetActive(true);
        //}
        observee = null;
        observeeName = "";
        OVRCameraRig.transform.position = Vector3.zero;
        perspPlaneMR.enabled = false;
        isObserving = false;
    }
    public float damping = 1f;
    public Vector3 observeOffset;
    void UpdateObservingPos()
    {
        if (isObserving && observee != null)
        {
            // not sure
            //Camera.main.transform.localPosition = Vector3.zero;
            Vector3 finalPos;
            float angle = observee.rotation.eulerAngles.y;
            Vector3 rotatedOffset = Quaternion.Euler(0, angle, 0) * observeOffset;
            switch (GlobalToggleIns.GetInstance().perspMode)
            {
                case GlobalToggle.ObserveMode.FPP:
                    finalPos = observee.position;
                    OVRCameraRig.transform.position = Vector3.Lerp(OVRCameraRig.transform.position, finalPos, Time.deltaTime * damping);
                    break;
                case GlobalToggle.ObserveMode.TPP:
                    finalPos = observee.position - rotatedOffset;
                    OVRCameraRig.transform.position = Vector3.Lerp(OVRCameraRig.transform.position, finalPos, Time.deltaTime * damping);
                    break;
                case GlobalToggle.ObserveMode.RT:
                    // enable RTCamera
                    finalPos = observee.position - rotatedOffset;
                    RTCamera.position = observee.position;
                    RTCamera.forward = observee.forward;
                    break;
                default:
                    break;
            }

            //Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, finalPos, Time.deltaTime * damping);
            // if we want the camera to look at the person
            //OVRCameraRig.transform.LookAt(target.transform);
        }
        else
        {
            //perspPlane.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void Update()
    {
        UpdateObservingPos();
        UpdateObservingMirror();
    }

    void UpdateObservingMirror()
    {
        // we need to duplicate observee, without flipping
    }
}

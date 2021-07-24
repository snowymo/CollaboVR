using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script controls the teleportation of the camera and controllers to a new location (ex if someone
//wanted to view someone else's board...). In the scene I have a target that is used as the teleportation
//destination. I put some cubes surrounding target to give some visual indication of being
//somewhere else in the scene. Basically the cubes are just for demonstration. This script should
//be attatched to the object that will be teleporting in the Init script! Also I noticed something weird where if that
//SteamVR popup comes up, you have to X out of it otherwise the keyboard input will not work -Nina

public class Teleport : MonoBehaviour
{
    public Transform newLocation; //a new location to teleport to for debugging purposes
    private Transform _originLocation; //your original location

    private OVRManager _manager;

    private OculusInput inputDevice;

    GameObject localAvatar;
    OculusManager om;


    public TransitionUtility.ColorInterp interp;

    public TransitionUtility.TransitionOverlay transitionOverlay;

    public GlowObjectCmd glowOutlineCommand;


    private GameObject[] remoteAvatars = new GameObject[0];

    public bool InitRemoteAvatars()
    {
        remoteAvatars = GameObject.FindGameObjectsWithTag("mws_avatar");
        if (remoteAvatars == null) {
            return false;
        }
        else if (remoteAvatars.Length == 0) {
            return false;
        }
        else {
            Debug.Log("<color=green>FOUND AVATARS</color>");
            return true;
        }
    }

    private void Start()
    {
        _originLocation = transform.parent.transform;
        _manager = gameObject.GetComponent<OVRManager>();

        GameObject inputDeviceObject = GameObject.Find("oculusController");
        if (inputDeviceObject == null) {
            Debug.LogError("Cannot find input device");
        }
        inputDevice = inputDeviceObject.GetComponent<OculusInput>();

        localAvatar = GameObject.Find("LocalAvatar");

        om = localAvatar.GetComponent<OculusManager>();



        //transitionOverlay
    }


    bool OculusDoTeleport()
    {
        switch (inputDevice.activeController) {
            case OVRInput.Controller.LTouch:
                return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
            case OVRInput.Controller.RTouch:
                return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch);
        }

        return false;
    }

    void EnablePositionTracking()
    {
        _manager.usePositionTracking = true; //turn on position tracking to unlock movement
        //_manager.useIPDInPositionTracking = true;
    }

    void DisablePositionTracking()
    {
        _manager.usePositionTracking = false; //turn off position tracking to lock movement
        //_manager.useIPDInPositionTracking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) || OculusDoTeleport()) { //teleport if the T key is pressed
            if (om.isObserving) { //move back to the start location if we are elsewhere
                UpdatePosition(_originLocation);
                //EnablePositionTracking();
                Debug.Log("Moving back to the start location");
                om.isObserving = false;
                //gameObject.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));

                if (remoteAvatars.Length > 0)
                {
                    remoteAvatars[0].SetActive(true);
                }

                localAvatar.transform.position = _originLocation.position;
            }
            else { //move to a new location!!
                if (om.remoteNames.Count > 0) {

                    Debug.Log("trying to get remote data");

                    SyncUserData remoteData;
                    if (om.usernameToUserDataMap.TryGetValue(om.remoteNames[0], out remoteData)) {
                        Debug.Log("Got remote location data");
                        // TODO only first remote person now, later need to choose the person who is currently drawing

                        {
                            newLocation.position = remoteData.position;
                            //newLocation.rotation = remoteData.rotation;
                        }

                        //newLocation = testObj;
                        UpdatePosition(newLocation);
                        DisablePositionTracking();
                        Debug.Log("Moving to new location");
                        om.isObserving = true;
                        //gameObject.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));

                        localAvatar.transform.position = newLocation.position;
                    }

                    if (remoteAvatars.Length > 0)
                    {
                        remoteAvatars[0].SetActive(false);
                    }
                }
            }
        }

        if (om.remoteNames.Count == 0)
        {
            return;
        }
        if (om.isObserving) {
            SyncUserData remoteData;
            Debug.Log("trying to get remote data");
            if (om.usernameToUserDataMap.TryGetValue(om.remoteNames[0], out remoteData)) {
                Debug.Log("Updating location with remote data");

                {
                    newLocation.position = remoteData.position;
                    //newLocation.rotation = remoteData.rotation;
                }

                UpdatePosition(newLocation);
            }
        }
        else {
            SyncUserData remoteData;

            Debug.Log("<color=red>Observation off, checking for whether other avatar is observing</color>");
            if (om.usernameToUserDataMap.TryGetValue(om.remoteNames[0], out remoteData)) {
                if (remoteAvatars.Length > 0)
                {
                    if (remoteData.UserIsObserving())
                    {
                        Debug.Log("<color=red>other user is observing</color>");
                        remoteAvatars[0].SetActive(false);
                    }
                    else
                    {
                        Debug.Log("<color=red>other user is NOT observing</color>");
                        remoteAvatars[0].SetActive(true);
                    }
                    Debug.Log("<color=red>user was " + remoteAvatars[0].gameObject.name + "</color>");
                }
            }
        }


        { // temp
            //Color transitionColor = interp.interpProc(transitionOverlay.startColor, transitionOverlay.endColor, interp.timeElapsed / interp.timeDuration);
            //transitionOverlay.UpdateColor(transitionColor);
            //interp.timeElapsed += Time.deltaTime;
            //if (interp.timeElapsed >= interp.timeDuration) {
            //    interp.ReSet();
            //}
        }
    }

    //This function just updates the camera (gameObject) transform to have the same rotation and position as
    //the new location that we are teleporting to.
    private void UpdatePosition(Transform t)
    {
        Debug.Log("<color=red>" + t.transform.position);
        gameObject.transform.position = t.transform.position;
        //gameObject.transform.rotation = t.transform.rotation;
    }

}
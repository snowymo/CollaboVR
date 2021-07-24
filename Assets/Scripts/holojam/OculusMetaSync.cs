using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusMetaSync : Holojam.Tools.SynchronizableTrackable
{

    public string label = "Meta";
    [SerializeField] string scope = "";

    //[SerializeField] bool host = true;
    [SerializeField] bool autoHost = false;

    public bool isLocal; // if it is local, packet the information and send, if it is remote, receive and decode the information
    public OvrAvatar ovrAvatar;
    public PerspectiveView perspView;
    public OculusManager om;

    public bool isTracked;
    BoxCollider bc;
    Vector3 predefinedCenter = new Vector3(0, -0.1302501f, -0.04994515f);

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return isLocal; } }
    public override bool AutoHost { get { return autoHost; } }

    protected override void Sync()
    {
        if (label == "Meta")
            label = GlobalToggleIns.GetInstance().username + "Meta";

        if (isLocal) {
            data.vector3s[0] = Camera.main.transform.position;
            data.vector3s[1] = Camera.main.transform.forward;
            data.vector4s[0] = Camera.main.transform.rotation;

            int flags = 0;
            if (perspView.isObserving) {
                SyncUserData.MarkUserIsObserving(ref flags);
            }
            data.ints[0] = flags; // TODO
        }
        if (!isLocal && Tracked) {
            SyncUserData remoteData;
            string remoteName = label.Substring(0, label.Length - 4);
            if (om.usernameToUserDataMap.TryGetValue(remoteName, out remoteData)) {
                // udpate
                remoteData.position = data.vector3s[0];
                remoteData.forward = data.vector3s[1];
                remoteData.rotation = data.vector4s[0];
                remoteData.flags = data.ints[0];
            }
            else {
                // add entry
                om.usernameToUserDataMap.Add(label.Substring(0, label.Length - 4), new SyncUserData(data.vector3s[0], data.vector3s[1], data.vector4s[0], data.ints[0]));
            }
            om.usernameToUserDataMap.TryGetValue(remoteName, out remoteData);
            // disable if it is in observe mode
            if (remoteData.UserIsObserving() && GlobalToggleIns.GetInstance().perspMode != GlobalToggle.ObserveMode.RT) {
                // we should not see the person who is observing others when FPP or TPP
                ovrAvatar.ShowThirdPerson = false;  
            }
            else {
                ovrAvatar.ShowThirdPerson = true;
            }
            // udpate collider with the pos
            bc.center = predefinedCenter + remoteData.position;
        }

        isTracked = Tracked;
    }

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(2, 1, 0, 1, 0, false);

        //localAvatarGameObject = GameObject.Find("LocalAvatar");
        //localAvatar = localAvatarGameObject.GetComponent<OvrAvatar>();
        perspView = GameObject.Find("LocalAvatar").GetComponent<PerspectiveView>();
        om = GameObject.Find("LocalAvatar").GetComponent<OculusManager>();
        ovrAvatar = GetComponent<OvrAvatar>();
        bc = gameObject.GetComponent<BoxCollider>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLeave : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnDestroy()
    {
        print("SendLeave bye bye");

        MSGSenderIns.GetIns().sender.Add(CommandToServer.AVATAR_LEAVE, GlobalToggleIns.GetInstance().username, "0");//msgSender.Add(3, curusername, myAvatar.oculusUserID);

        //base.OnDestroy();
    }
}

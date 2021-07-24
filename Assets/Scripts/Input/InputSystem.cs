using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// (Karl) I would like to find a way to check for controls more "generically" because, for example,
// OculusInput would be annoying to rewrite every time for a different device. Maybe the device-specific
// objects like OculusInput could just fill a struct of data, and we define functions like "Draw" or "Toggle Controls" so the logic doesn't have to be copy-pasted everywhere
// low-priority for now, but maybe helpful in the long-run
// the following is just a start/test
public class InputSystem {
    public enum DeviceType
    {
        UNKNOWN,
        OCULUS,
    }

    public DeviceType devType;
    public GameObject inputHandler;
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipLineCtrl : MonoBehaviour {

    LineRenderer lr;

	// Use this for initialization
	void Start () {
        lr.GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        lr.useWorldSpace = false;
    }
}

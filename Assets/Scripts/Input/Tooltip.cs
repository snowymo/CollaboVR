using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour {

    // change the text when necessary

    bool isVisible;

    public bool isDominant;

    public TMPro.TextMeshPro drawToggleText, one, two, move, draw;

    Transform[] children;

    Transform helpCylinder;
    MeshRenderer helpCylinderMR;
    //public Vector3 ButtonOne, ButtonTwo, PrimaryIndex, SecondaryIndex, PrimaryHand, SecondaryHand, PrimaryThumbstick, SecondaryThumbstick;
    //public Vector3 drawToggleEuler;
    //public float drawToggleScale;
    //public Vector3 drawTogglePos;
    //public GameObject drawToggle;
    // Use this for initialization
    void Start() {
        //drawToggleScale = Vector3.one;
        isVisible = true;
        //children = GameObject.find
        if (isDominant) {
            one.text = "Select";
            two.text = "Create Board";
            move.text = "Move Sketch";
        }
        else {
            one.text = "Observe";
            two.text = "Help";
            move.text = "Move Observation";
        }
        helpCylinder = transform.Find("Cylinder");
        helpCylinderMR = helpCylinder.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    
    public void ToggleTooltip(){
        isVisible = !isVisible;
        if (isVisible) {
            foreach (Transform child in transform) {
                child.gameObject.SetActive(true);
            }
            // turn off cylinder
            helpCylinderMR.enabled = false;
        }
        else {
            foreach (Transform child in transform) {
                child.gameObject.SetActive(false);
            }
            // turn on cylinder
            if (!isDominant) {
                helpCylinderMR.enabled = true;
                helpCylinder.gameObject.SetActive(true);
            }                
        }
    }

    public void SwitchDominantHand(bool isDom)
    {
        if(isDom != isDominant) {
            isDominant = isDom;
            if (isDominant) {
                one.text = "Select";
                two.text = "Create Board";
                move.text = "Move Sketch";
            }
            else {
                one.text = "Observe";
                two.text = "Help";
                move.text = "Move Observation";
            }
        }
    }
}

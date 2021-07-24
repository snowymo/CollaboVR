using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinusoidalMovement : MonoBehaviour {
    private void Start()
    {
    }
    // Update is called once per frame
    void Update () {
        this.transform.Translate(new Vector3(Mathf.Sin(Time.time) * Time.deltaTime, 0.0f, 0.0f));
	}
}

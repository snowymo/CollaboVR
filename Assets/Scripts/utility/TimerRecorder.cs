using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class TimerRecorder : MonoBehaviour {

    public float durationInSec;

    public TMPro.TextMeshPro countdownText;

    public bool bShowText = false;

    List<float> records;

    float lastTimeCountDown = 0;

    // Use this for initialization
    void Start () {
        countdownText = GetComponent<TMPro.TextMeshPro>();
        records = new List<float>();
    }
	
	// Update is called once per frame
	void Update () {
        if (bShowText) {
            countdownText.text = ((int)durationInSec / 60).ToString() + ":" + ((int)durationInSec % 60).ToString();
        }
        else {
            countdownText.text = "";
        }
	}

    public void StartCountDown(int duration)
    {
        if(Mathf.Abs(durationInSec - duration * 60f) >= 5f) {
            durationInSec = duration * 60f;
            print("duration in sec:" + durationInSec);
            bShowText = true;
            StartCoroutine("LoseTime");
            lastTimeCountDown = Time.time;
        }        
    }

    public void addTimeRecord()
    {
        records.Add(Time.time);
    }

    //Simple Coroutine
    IEnumerator LoseTime()
    {
        while (durationInSec > 0) {
            yield return new WaitForSeconds(1);
            durationInSec--;
        }
    }

    public void finalize()
    {
        // create a file with username and real time
        string path = "Assets/Outputs/" + GlobalToggleIns.GetInstance().username + "_" + System.DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ","") + ".csv";

        //Write some text to the test.txt file
        StreamWriter writer = File.CreateText(path);
        writer.WriteLine(lastTimeCountDown + ",\n");
        for (int i = 0; i < records.Count; i++)
            writer.WriteLine(records[i]+",\n");
        writer.Close();

        print("write to " + path);
    }
}

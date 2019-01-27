using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialgoueController : MonoBehaviour {
    public static DialgoueController instance;
    public List<string> scripts;
    public List<string> speakerName;
	// Use this for initialization
	void Start () {
        instance = this;
        spk.text = speakerName[curr];
        text.FinalText = scripts[curr];
        text.reset = true;
        text.On = true;
	}
    public void say(){
        GetComponent<Image>().enabled=true;
        transform.GetChild(0).gameObject.SetActive((true));
        transform.GetChild(1).gameObject.SetActive((true));
        curr = 0;

    }
    public int curr = 0;
    public TypeOutScript text;
    public Text spk;
    bool opening = true;
	// Update is called once per frame

    void Next(){
        curr++;
        if (curr == 4)
        {
            Debug.Log("turnoff");
            transform.GetChild(0).gameObject.SetActive((false));
            transform.GetChild(1).gameObject.SetActive((false)); 
            opening = false;
            GetComponent<Image>().enabled = false;
            return;
        }
        text.FinalText = scripts[curr];
        spk.text = speakerName[curr];
        text.reset = true;
        text.On = true;

    }
	void Update () {
        
        if (!opening) { return; }
        if (Input.GetKeyUp((KeyCode.Space)))
        {

            Next();
        }else if (Input.GetKeyUp(KeyCode.Return)){

        text.On = true;

        }
	}
}

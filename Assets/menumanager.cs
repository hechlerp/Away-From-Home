using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menumanager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	public void startgame() {
        Application.LoadLevel("Main");
    }
	// Update is called once per frame
	void Update () {
	if (Input.GetKeyUp(KeyCode.Space)) {
            startgame();

        }	
	}
}

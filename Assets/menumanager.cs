﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menumanager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	public void startgame() {
        SceneManager.LoadSceneAsync("peter dev scene");
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Space)) {
            startgame();

        } else if (Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        }
	} 
}

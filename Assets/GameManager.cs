using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
	// Use this for initialization
	void Start () {
        instance = this;
	}
    public void Win(){

        winPage.SetActive((true));
        PlayerController.instance.isActive = false;
    }
   public GameObject failPage;
   public GameObject winPage;
    public void Fail(){

        failPage.SetActive((true));
        PlayerController.instance.isActive = false;
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.BackQuote)) {
            restartLevel();
        }
        if (Input.GetKeyUp(KeyCode.F5)) {
            saveProgress();
        }
	}

    void restartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void saveProgress() {
        ProgressManager.saveProgress(SceneManager.GetActiveScene().name);
    }
}

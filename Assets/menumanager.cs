using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class menumanager : MonoBehaviour {

    List<System.Action> actions;
    public List<Button> buttons;
    public Image highlighter;
    int selectedButtonIndex;
    // Use this for initialization
    void Start() {
        actions = new List<System.Action>() {
            startgame,
            goToLevelSelector,
            goToControls
        };
        selectedButtonIndex = 0;
        updateHighlighterPos();
    }
    public void startgame() {
        SceneManager.LoadSceneAsync("RenderingDevScene");
    }

    public void goToControls() {
        SceneManager.LoadSceneAsync("ControlsScene");
    }

    public void goToLevelSelector() {
        SceneManager.LoadSceneAsync("LevelSelector");
    }

    void updateHighlighterPos() {
        highlighter.transform.position = buttons[selectedButtonIndex].transform.position;
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Space)) {
            startgame();

        } else if (Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        } else if (Input.GetKeyUp(KeyCode.DownArrow) | Input.GetKeyUp(KeyCode.S)) {
            if (selectedButtonIndex == 2) {
                selectedButtonIndex = 0;
            } else {
                selectedButtonIndex++;
            }
            updateHighlighterPos();
        } else if (Input.GetKeyUp(KeyCode.UpArrow) | Input.GetKeyUp(KeyCode.W)) {
            if (selectedButtonIndex == 0) {
                selectedButtonIndex = 2;
            } else {
                selectedButtonIndex--;
            }
            updateHighlighterPos();
        } else if (Input.GetKeyUp(KeyCode.Return)) {
            actions[selectedButtonIndex]();
        }
	} 
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PopulateAvailableLevels : MonoBehaviour
{
    void Start()
    {
        Progress progress = ProgressManager.readProgress();
        Dictionary<string, Dictionary<string, object>> levelData = LevelData.levelData;
        Dictionary<string, object> currentMaxScene = levelData[progress.maxProgress];
        int maxScenePosition = (int)currentMaxScene["position"];

        // the current structure assumes a maximum of 6 levels. When that changes, pagination will have to be implemented.
        GameObject levelBoxContainer = GameObject.Find("LevelBoxContainer");
        List<GameObject> levelBoxes = new List<GameObject>();
        foreach(Transform child in levelBoxContainer.transform) {
            levelBoxes.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        foreach(KeyValuePair<string, Dictionary<string, object>> entry in levelData) {
            int entryPosition = (int)entry.Value["position"];
            string entryLabel = (string)entry.Value["label"];
            if (entryPosition <= maxScenePosition) {
                GameObject levelBox = levelBoxes[entryPosition];
                levelBox.GetComponentInChildren<Text>().text = entryLabel;
                string spritePath = "UI/LevelImages/" + entry.Key;
                Sprite levelSprite = Resources.Load<Sprite>(spritePath);
                levelBox.GetComponentInChildren<Image>().sprite = levelSprite;
                UnityAction loadPassedScene = () => {
                    SceneManager.LoadSceneAsync(entry.Key);
                };
                levelBox.GetComponentInChildren<Button>().onClick.AddListener(loadPassedScene);
                levelBox.SetActive(true);
            }
        }
        
    }
}

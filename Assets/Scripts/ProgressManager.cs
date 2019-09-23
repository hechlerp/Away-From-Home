using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ProgressManager {
    static string savePath = "Assets/Resources/SavedProgress/savedProgress.txt";

    public static void saveProgress(string sceneName) {
        StreamReader reader = new StreamReader(savePath, true);
        string prevProgressString = reader.ReadToEnd();
        reader.Close();
        Progress prevProgress = JsonUtility.FromJson<Progress>(prevProgressString);
        int prevMaxProgress = 0;
        string updatedMaxProgress = "";
        bool isCurrentSceneFurther = true;
        if (prevProgress.maxProgress != "") {
            Dictionary<string, Dictionary<string, object>> levelData = LevelData.levelData;
            prevMaxProgress = (int)levelData[prevProgress.maxProgress]["position"];
            updatedMaxProgress = prevProgress.maxProgress;
            isCurrentSceneFurther = prevMaxProgress < (int)levelData[sceneName]["position"];
        }
        if (isCurrentSceneFurther) {
            updatedMaxProgress = sceneName;
        }

        StreamWriter writer = new StreamWriter(savePath, false);
        Progress progress = new Progress();
        progress.currentProgress = sceneName;
        progress.maxProgress = updatedMaxProgress;
        string stringifiedProgress = JsonUtility.ToJson(progress);
        writer.Write(stringifiedProgress);
        writer.Close();
    }

    public static Progress readProgress() {
        StreamReader reader = new StreamReader(savePath, true);
        string progressString = reader.ReadToEnd();
        reader.Close();
        Progress prevProgress = JsonUtility.FromJson<Progress>(progressString);
        return prevProgress;
    }
}

public class Progress {
    public string currentProgress;
    public string maxProgress;
}
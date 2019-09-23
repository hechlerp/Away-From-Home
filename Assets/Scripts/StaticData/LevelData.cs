using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    public static Dictionary<string, Dictionary<string, object>> levelData = new Dictionary<string, Dictionary<string, object>>() {
        {
            "RenderingDevScene", new Dictionary<string, object>() {
                {"position", 0},
                {"label", "Outskirts of Tenochtitlan"}
            }
        },
        {
            "ReloadScene", new Dictionary<string, object>() {
                {"position", 1},
                {"label", "Test scene 2"}
            }
        }
    };
}

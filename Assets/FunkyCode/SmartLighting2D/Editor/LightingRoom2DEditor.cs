using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LightingRoom2D))]
public class LightingRoom2DEditor :Editor {
    	override public void OnInspectorGUI() {
		    LightingRoom2D script = target as LightingRoom2D;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup("Preset", LightingManager2D.Get().preset);
            EditorGUI.EndDisabledGroup();
            
            script.color = EditorGUILayout.ColorField("Color", script.color);
        }
}

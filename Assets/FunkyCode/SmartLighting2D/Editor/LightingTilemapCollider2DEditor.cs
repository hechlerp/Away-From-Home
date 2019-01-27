using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LightingTilemapCollider2D))]
public class LightingTilemapCollider2DEditor : Editor {
	override public void OnInspectorGUI() {
		LightingTilemapCollider2D script = target as LightingTilemapCollider2D;

		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.EnumPopup("Preset", LightingManager2D.Get().preset);
		EditorGUI.EndDisabledGroup();

		script.mapType = (LightingTilemapCollider2D.MapType)EditorGUILayout.EnumPopup("Tilemap Type", script.mapType);

		script.colliderType = (LightingTilemapCollider2D.ColliderType)EditorGUILayout.EnumPopup("Collision Type", script.colliderType);
		script.lightingCollisionLayer = (LightingLayer)EditorGUILayout.EnumPopup("Collision Layer", script.lightingCollisionLayer);
		script.maskType = (LightingTilemapCollider2D.MaskType)EditorGUILayout.EnumPopup("Mask Type", script.maskType);
		script.lightingMaskLayer = (LightingLayer)EditorGUILayout.EnumPopup("Mask Layer", script.lightingMaskLayer);

		
		script.dayHeight = EditorGUILayout.Toggle("Day Height", script.dayHeight);
		if (script.dayHeight)  {
			script.height = EditorGUILayout.FloatField("Height", script.height);
		}
		
		//script.ambientOcclusion = EditorGUILayout.Toggle("Ambient Occlusion", script.ambientOcclusion);
		//if (script.ambientOcclusion)  {
		//	script.occlusionSize = EditorGUILayout.FloatField("Occlussion Size", script.occlusionSize);
		//}

		script.batching = EditorGUILayout.Toggle("Batch Sprite Masking", script.batching);

		if (GUILayout.Button("Update Collisions")) {
			script.Initialize();

			foreach(LightingSource2D light in LightingSource2D.GetList()) {
				light.update = true;
			}

			LightingMainBuffer2D.ForceUpdate();
		}
		
		if (GUI.changed && EditorApplication.isPlaying == false){
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}
	}
}

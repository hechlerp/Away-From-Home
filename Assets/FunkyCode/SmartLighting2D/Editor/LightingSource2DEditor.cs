using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

[CanEditMultipleObjects]
[CustomEditor(typeof(LightingSource2D))]
public class LightingSource2DEditor : Editor {
	static bool foldout = true;

	override public void OnInspectorGUI() {
		LightingSource2D script = target as LightingSource2D;

		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.EnumPopup("Preset", LightingManager2D.Get().preset);
		EditorGUI.EndDisabledGroup();

		//script.lightingCollisionLayer = (LightingCollisionLayer)EditorGUILayout.EnumPopup("Collision Layer", script.lightingCollisionLayer);
		//script.lightingMaskLayer = (LightingMaskLayer)EditorGUILayout.EnumPopup("Mask Layer", script.lightingMaskLayer);
		script.layerCount = EditorGUILayout.IntField("Layer Count", script.layerCount);

		foldout = EditorGUILayout.Foldout(foldout, "Layers" );
		if (foldout) {
			EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
			for(int i = 0; i < script.layerCount; i++) {
				if (script.layerSetting.Length <= i) {
					System.Array.Resize(ref script.layerSetting, i + 1);
				}
				if (script.layerSetting[i] == null) {
					script.layerSetting[i] = new LayerSetting();
				}
				script.layerSetting[i].layerID = (LightingLayer)EditorGUILayout.EnumPopup("Layer ID", script.layerSetting[i].layerID);
				script.layerSetting[i].renderingOrder = (LightRenderingOrder)EditorGUILayout.EnumPopup("Order", script.layerSetting[i].renderingOrder);
			}
			EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
		}

		Color newColor = EditorGUILayout.ColorField("Color", script.lightColor);
		
		if (script.lightColor.Equals(newColor) == false) {
			newColor.a = 1f;
			script.lightColor = newColor;
			
			LightingMainBuffer2D.ForceUpdate();
		}

		float newAlpha = EditorGUILayout.Slider("Alpha", script.lightAlpha, 0, 1);
		if (script.lightAlpha != newAlpha) {
			script.lightAlpha = newAlpha;

			LightingMainBuffer2D.ForceUpdate();
		}

		float newLightSize = EditorGUILayout.FloatField("Size", script.lightSize);
		if (newLightSize != script.lightSize) {
			script.lightSize = newLightSize;

			LightingMainBuffer2D.ForceUpdate();
		}
		
		switch(LightingManager2D.Get().fixedLightBufferSize) {
			case true:
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.EnumPopup("Buffer Size", LightingManager2D.Get().fixedLightTextureSize);
				EditorGUI.EndDisabledGroup();

				break;

			case false:
				script.textureSize = (LightingSourceTextureSize)EditorGUILayout.EnumPopup("Buffer Size", script.textureSize);
				break;
		}

		script.lightSprite = (LightingSource2D.LightSprite)EditorGUILayout.EnumPopup("Light Sprite", script.lightSprite);

		if (script.lightSprite == LightingSource2D.LightSprite.Custom) {
			Sprite newSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", script.sprite, typeof(Sprite), true);
			if (newSprite != script.sprite) {
				script.sprite = newSprite;
				script.SetMaterial();

				LightingMainBuffer2D.ForceUpdate();
			}
		} else {
			if (script.sprite != LightingSource2D.GetDefaultSprite()) {
				script.sprite = LightingSource2D.GetDefaultSprite();
				script.SetMaterial();

				LightingMainBuffer2D.ForceUpdate();
			}
		}
	
		script.enableCollisions = EditorGUILayout.Toggle("Apply Shadows & Masks", script.enableCollisions);

		script.rotationEnabled = EditorGUILayout.Toggle("Apply Rotation", script.rotationEnabled);

		script.additive = EditorGUILayout.Toggle("Apply Additive Shader", script.additive);
		
		if (script.additive) {
			script.additive_alpha = EditorGUILayout.Slider("Additive Alpha", script.additive_alpha, 0, 1);
		}

		script.eventHandling = EditorGUILayout.Toggle("Apply Event Handling" , script.eventHandling);

		script.drawInsideCollider = EditorGUILayout.Toggle("Apply Light Inside Collider", script.drawInsideCollider);

		if (targets.Length > 1) {
			if (GUILayout.Button("Apply to All")) {
				foreach(Object obj in targets) {
					LightingSource2D copy = obj as LightingSource2D;
					if (copy == script) {
						continue;
					}


					copy.layerSetting[0].renderingOrder = script.layerSetting[0].renderingOrder;
					copy.layerSetting[1].renderingOrder = script.layerSetting[1].renderingOrder;
					
					copy.lightColor = script.lightColor;
					copy.lightAlpha = script.lightAlpha;
					copy.lightSize = script.lightSize;
					copy.textureSize = script.textureSize;

					copy.enableCollisions = script.enableCollisions;
					copy.rotationEnabled = script.rotationEnabled;
					copy.additive = script.additive;
					copy.additive_alpha = script.additive_alpha;
					

					copy.eventHandling = script.eventHandling;
					copy.drawInsideCollider = script.drawInsideCollider;

					copy.lightSprite = script.lightSprite;
					copy.sprite = script.sprite;
					copy.SetMaterial();
				}

				LightingMainBuffer2D.ForceUpdate();
			}
		}
		
		if (GUI.changed && EditorApplication.isPlaying == false){
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}
	}
}

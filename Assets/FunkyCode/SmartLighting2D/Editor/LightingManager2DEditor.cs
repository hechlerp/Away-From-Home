using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LightingManager2D))]
public class LightingManager2DEditor : Editor {

	[MenuItem("GameObject/2D Light/Light Source", false, 4)]
    static void CreateLightSource(){
		Ray worldRay = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
		
		GameObject newGameObject = new GameObject("2D Light Source");

		Vector3 pos = worldRay.origin;
		pos.z = 0;

		newGameObject.AddComponent<LightingSource2D>();

		newGameObject.transform.position = pos;
	}

	[MenuItem("GameObject/2D Light/Light Collider", false, 4)]
    static void CreateLightCollider(){
		Ray worldRay = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
		
		GameObject newGameObject = new GameObject("2D Light Collider");

		Vector3 pos = worldRay.origin;
		pos.z = 0;

		newGameObject.AddComponent<PolygonCollider2D>();

		newGameObject.AddComponent<LightingCollider2D>();

		newGameObject.transform.position = pos;
    }

	[MenuItem("GameObject/2D Light/Light Tilemap Collider", false, 4)]
    static void CreateLightTilemapCollider(){
		GameObject newGrid = new GameObject("2D Light Grid");
		newGrid.AddComponent<Grid>();

		GameObject newGameObject = new GameObject("2D Light Tilemap");
		newGameObject.transform.parent = newGrid.transform;

		newGameObject.AddComponent<Tilemap>();
		newGameObject.AddComponent<LightingTilemapCollider2D>();
    }

	[MenuItem("GameObject/2D Light/Light Sprite Renderer", false, 4)]
    static void CreateLightSpriteRenderer(){
		Ray worldRay = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
		
		GameObject newGameObject = new GameObject("2D Light Sprite Renderer");

		Vector3 pos = worldRay.origin;
		pos.z = 0;

		newGameObject.AddComponent<LightingSpriteRenderer2D>();

		newGameObject.transform.position = pos;
    }

	override public void OnInspectorGUI() {
		LightingManager2D script = target as LightingManager2D;

		EditorGUI.BeginDisabledGroup(true);
		script.preset = LightingManager2D.Preset.Default;
		EditorGUILayout.EnumPopup("Preset", LightingManager2D.Preset.Default);
		EditorGUI.EndDisabledGroup();

		script.renderingMode = (LightingManager2D.RenderingMode)EditorGUILayout.EnumPopup("Rendering Mode", script.renderingMode);

		if (script.renderingMode == LightingManager2D.RenderingMode.MeshRenderer) {
			script.sortingLayerID = (int)EditorGUILayout.Slider("Sorting Layer ID", script.sortingLayerID, 0, 31);
			script.sortingLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", script.sortingLayerOrder);
			script.sortingLayerName = EditorGUILayout.TextField("Sorting Layer Name", script.sortingLayerName);
		}

		Color newDarknessColor = EditorGUILayout.ColorField("Darkness Color", script.darknessColor);
		if (script.darknessColor.Equals(newDarknessColor) == false) {
			script.darknessColor = newDarknessColor;

			LightingMainBuffer2D.ForceUpdate();	
		}

		float newShadowDarkness = EditorGUILayout.Slider("Shadow Darkness", script.shadowDarkness, 0, 1);
		if (newShadowDarkness != script.shadowDarkness) {
			script.shadowDarkness = newShadowDarkness;

			LightingMainBuffer2D.ForceUpdate();
		}
		
		float newSunDirection = EditorGUILayout.FloatField("Sun Rotation", script.sunDirection);
		if (newSunDirection != script.sunDirection) {
			script.sunDirection = newSunDirection;

			LightingMainBuffer2D.ForceUpdate();
		}

		script.drawAdditiveLights = EditorGUILayout.Toggle("Draw Additive Lights", script.drawAdditiveLights);

		script.drawRooms = EditorGUILayout.Toggle("Draw Rooms", script.drawRooms);

		script.drawOcclusion = EditorGUILayout.Toggle("Draw Occlusion", script.drawOcclusion);

		script.drawDayShadows = EditorGUILayout.Toggle("Draw Day Shadows", script.drawDayShadows);

		script.darknessBuffer = EditorGUILayout.Toggle("Draw Main Buffer", script.darknessBuffer);

		script.drawSceneView = EditorGUILayout.Toggle("Draw Scene View", script.drawSceneView);

		script.lightingResolution = EditorGUILayout.Slider("Lighting Resolution", script.lightingResolution, 0.125f, 1.0f);
		
		script.fixedLightBufferSize = EditorGUILayout.Toggle("Fixed Light Buffer", script.fixedLightBufferSize);
	
		if (script.fixedLightBufferSize) {
			script.fixedLightTextureSize = (LightingSourceTextureSize)EditorGUILayout.EnumPopup("Fixed Light Buffer Size", script.fixedLightTextureSize);
		} else {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.EnumPopup("Fixed Light Buffer Size", script.fixedLightTextureSize);
			EditorGUI.EndDisabledGroup();
		}

		script.batchColliderMask = EditorGUILayout.Toggle("Batch Collider Mask", script.batchColliderMask);
		
		script.debug = EditorGUILayout.Toggle("Debug", script.debug);
		script.disableEngine = EditorGUILayout.Toggle("Disable Engine", script.disableEngine);

		string buttonName = "Re-Initialize";
		if (script.version < LightingManager2D.VERSION) {
			buttonName += " (Outdated)";
			GUI.backgroundColor = Color.red;
		}
		
		if (GUILayout.Button(buttonName)) {
			if (script.mainBuffer != null && script.fboManager != null && script.meshRendererMode != null) {
				DestroyImmediate(script.mainBuffer.gameObject);
				DestroyImmediate(script.fboManager.gameObject);
				DestroyImmediate(script.meshRendererMode.gameObject);

				script.Initialize();
			} else {
				DestroyImmediate(script.gameObject);
				LightingManager2D.Get();
			}
			
			LightingMainBuffer2D.ForceUpdate();
		}

		if (GUI.changed && EditorApplication.isPlaying == false) {
			if (target != null) {
				EditorUtility.SetDirty(target);
			}
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}
	}
}
 
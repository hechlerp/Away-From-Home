using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

[ExecuteInEditMode] // Only 1 Lighting Manager Allowed
public class LightingManager2D : MonoBehaviour {
	public enum RenderingMode {OnPreRender, OnPostRender, MeshRenderer}
	public enum Preset {Default, TopDown, SideScroller, Isometric};
	private static LightingManager2D instance;

	public RenderingMode renderingMode = RenderingMode.OnPostRender;

	// Mesh Render Settings

	public MeshRendererMode meshRendererMode = null;
	public int sortingLayerID;
	public string sortingLayerName;
	public int sortingLayerOrder;

	// Pre-Render Settings

	public Preset preset = Preset.Default;

	public Color darknessColor = Color.black;
	public float sunDirection = - Mathf.PI / 2; // Should't it represent degrees???
	public float shadowDarkness = 1;

	public bool drawDayShadows = true;
	public bool drawRooms = true;
	public bool drawOcclusion = true;
	public bool drawAdditiveLights = true;
	public bool darknessBuffer = true; // Draw Main Buffer

	public float lightingResolution = 1f;

	public bool fixedLightBufferSize = true;
	public LightingSourceTextureSize fixedLightTextureSize = LightingSourceTextureSize.px512;

	public bool batchColliderMask = false;

	public bool debug = false;
	public bool drawSceneView = false;
	public bool disableEngine = false;

	// Buffer Instance
	public LightingMainBuffer2D mainBuffer;

	// FBO Instance
	public FBOManager fboManager;

	// Materials
	public Material penumbraMaterial;
	public Material occlusionEdgeMaterial;
	public Material occlusionBlurMaterial;
	public Material shadowBlurMaterial;
	public Material additiveMaterial;

	public Material whiteSpriteMaterial;
	public Material blackSpriteMaterial;

	public const int lightingLayer = 8;
	public const bool culling = true;

	public int version = 0;
	public const int VERSION = 104;

	static public int GetTextureSize(LightingSourceTextureSize textureSize) {
		switch(textureSize) {
			case LightingSourceTextureSize.px2048:
				return(2048);

			case LightingSourceTextureSize.px1024:
				return(1024);

			case LightingSourceTextureSize.px512:
				return(512);

			case LightingSourceTextureSize.px256:
				return(256);
			
			default:
				return(128);
		}
	}

	public static Mesh preRenderMesh = null;
	public static Mesh GetRenderMesh() {
		if (preRenderMesh == null) {
			Polygon2D tilePoly = Polygon2D.CreateFromRect(new Vector2(1f, 1f));
			Mesh staticTileMesh = tilePoly.CreateMesh(new Vector2(2f, 2f), Vector2.zero);
			preRenderMesh = staticTileMesh;
		}
		return(preRenderMesh);
	}

	static public LightingManager2D Get() {
		if (instance != null) {
			return(instance);
		}

		foreach(LightingManager2D manager in Object.FindObjectsOfType(typeof(LightingManager2D))) {
			instance = manager;
			return(instance);
		}

		// Create New Light Manager
		GameObject gameObject = new GameObject();
		gameObject.name = "Lighting Manager 2D";

		instance = gameObject.AddComponent<LightingManager2D>();
		instance.Initialize();

		return(instance);
	}

	public void Initialize () {
		instance = this;

		// Lighting Materials
		additiveMaterial = new Material (Shader.Find (Max2D.shaderPath + "Particles/Additive"));
		
		penumbraMaterial = new Material (Shader.Find (Max2D.shaderPath + "Particles/Multiply"));
		penumbraMaterial.mainTexture = Resources.Load ("textures/penumbra") as Texture;

		occlusionEdgeMaterial = new Material (Shader.Find (Max2D.shaderPath + "Particles/Multiply"));
		occlusionEdgeMaterial.mainTexture = Resources.Load ("textures/occlusionedge") as Texture;

		shadowBlurMaterial = new Material (Shader.Find (Max2D.shaderPath + "Particles/Multiply"));
		shadowBlurMaterial.mainTexture = Resources.Load ("textures/shadowblur") as Texture;

		occlusionBlurMaterial = new Material (Shader.Find (Max2D.shaderPath + "Particles/Multiply"));
		occlusionBlurMaterial.mainTexture = Resources.Load ("textures/occlussionblur") as Texture;

		whiteSpriteMaterial = new Material (Shader.Find ("SmartLighting2D/SpriteWhite"));

		blackSpriteMaterial = new Material (Shader.Find ("SmartLighting2D/SpriteBlack"));

		transform.position = Vector3.zero;

		mainBuffer = LightingMainBuffer2D.Get(); 

		fboManager = FBOManager.Get();

		version = VERSION;
	}

/* 
	void DrawLightingBuffers(float z) {
		
	}*/

			/* Draw Lights In Scene View
		Polygon2D rect = Polygon2D.CreateFromRect(new Vector2(1, 1));
		Mesh mesh = rect.CreateMesh(new Vector2(2, 2), Vector2.zero);

		
		foreach (LightingBuffer2D id in FBOManager.GetList()) {
			if (id.lightSource == null) {
				continue;
			}

			if (id.lightSource.isActiveAndEnabled == false) {
				continue;
			}

       		Draw(id, mesh);
		} */	
	/*
    private void Draw(LightingBuffer2D id, Mesh mesh) {
        if (mesh && id.material) {
			Color lightColor = id.lightSource.lightColor;
			lightColor.a = id.lightSource.lightAlpha / 4;
			id.material.SetColor ("_TintColor", lightColor);
			
			Graphics.DrawMesh(mesh, Matrix4x4.TRS(id.lightSource.transform.position, id.lightSource.transform.rotation,  id.lightSource.transform.lossyScale * id.lightSource.lightSize), id.material, 0);
        }
    } */

	void DrawAdditiveLights() {
		foreach (LightingSource2D id in LightingSource2D.GetList()) {
			if (id.buffer == null) {
				continue;
			}

			if (id.isActiveAndEnabled == false) {
				continue;
			}

			if (id.buffer.bufferCamera == null) {
				continue;
			}

			if (id.InCamera() == false) {
				continue;
			}

			if (id.additive == false) {
				continue;
			}

			if (id.buffer.additiveMaterial) {
				Color lightColor = id.lightColor;
				lightColor.a = 0.5f;
				lightColor.r *= id.additive_alpha;
				lightColor.g *= id.additive_alpha;
				lightColor.b *= id.additive_alpha;

				id.buffer.additiveMaterial.SetColor ("_TintColor", lightColor);
				
				Graphics.DrawMesh(GetRenderMesh(), Matrix4x4.TRS(id.transform.position, Quaternion.Euler(0, 0, 0),  id.transform.lossyScale * id.lightSize), id.buffer.additiveMaterial, 0);
			}
		}
	}

	void Update() {
		if (disableEngine) {
			mainBuffer.enabled = false;
			mainBuffer.bufferCamera.enabled = false;
			meshRendererMode.meshRenderer.enabled = false;
			return;
		}

		if (drawAdditiveLights) {
			DrawAdditiveLights();
		}

		if (Render_Check() == false) {
			return;
		}

		mainBuffer.darknessColor = darknessColor;

		if (darknessBuffer) {
			mainBuffer.enabled = true;
			mainBuffer.bufferCamera.enabled = true;
		} else {
			mainBuffer.enabled = false;
			mainBuffer.bufferCamera.enabled = false;
		}

		Render_MeshRenderMode();
	}

	// Pre-Render Mode Drawing
	public void LateUpdate() {
		if (disableEngine) {
			return;
		}
		Render_PreRenderMode();
	}

	// Post-Rendering Mode Drawing	
	public void OnRenderObject() {
		if (disableEngine) {
			return;
		}
		Render_PostRenderMode();
	}

	public bool Render_Check() {
		if (darknessBuffer == false) {
			return(false);
		}

		if (mainBuffer == null) {
			mainBuffer = LightingMainBuffer2D.Get();
			return(false);
		}	

		if (mainBuffer.bufferCamera == null) {
			return(false);
		}

		return(true);
	}

	public static Vector3 Render_Size() {
		float sizeX = Get().mainBuffer.bufferCamera.orthographicSize * ((float)Screen.width / Screen.height);
		Vector3 size = new Vector2(sizeX, Get().mainBuffer.bufferCamera.orthographicSize);
		
		size.x *= ((float)Screen.width / (float)Screen.height) / (size.x / size.y);
		size.z = 1;

		return(size);
	}

	void Render_MeshRenderMode() {
		// Mesh-Render Mode Drawing
		if (Render_Check() == false) {
			return;
		}

		meshRendererMode = MeshRendererMode.Get();
		if (renderingMode != RenderingMode.MeshRenderer) {
			meshRendererMode.meshRenderer.enabled = false;
			return;
		}

		meshRendererMode.meshRenderer.sortingLayerID = sortingLayerID;
		meshRendererMode.meshRenderer.sortingLayerName= sortingLayerName;
		meshRendererMode.meshRenderer.sortingOrder = sortingLayerOrder;

		meshRendererMode.meshRenderer.enabled = true;
		if (meshRendererMode.meshRenderer.sharedMaterial != mainBuffer.material) {
			meshRendererMode.meshRenderer.sharedMaterial = mainBuffer.material;
		}
	}

	void Render_PostRenderMode() {
		// Post-Render Mode Drawing
		if (Render_Check() == false) {
			return;
		}

		if (renderingMode != RenderingMode.OnPostRender) {
			return;
		}

		if (Camera.current != Camera.main) {
			return;
		}

		LightingDebug.LightMainCameraUpdates +=1 ;
		
		Vector3 pos = Camera.main.transform.position;
		pos.z += Camera.main.nearClipPlane + 0.1f;

		Max2D.DrawImage(mainBuffer.material, pos, Render_Size(), Camera.main.transform.eulerAngles.z, pos.z);
	}

	void Render_PreRenderMode() {
		if (Render_Check() == false) {
			return;
		}

		if (renderingMode != RenderingMode.OnPreRender) {
			return;
		}

		Vector3 pos = Camera.main.transform.position;
		pos.z += Camera.main.nearClipPlane + 0.1f;

		Quaternion rotation = Camera.main.transform.rotation;

		Graphics.DrawMesh(GetRenderMesh(), Matrix4x4.TRS(pos, rotation, Render_Size()), mainBuffer.material, 0);
	}

	static public float GetSunDirection() {
		return(Get().sunDirection);
	}

	void OnGUI() {
		if (debug) {
			LightingDebug.OnGUI();
		}
	}

	public class LightingDebug {
		static public int LightBufferUpdates = 0;
		static public int ShowLightBufferUpdates = 0;

		static public int LightMainBufferUpdates = 0;
		static public int ShowLightMainBufferUpdates = 0;

		static public int LightMainCameraUpdates = 0;
		static public int ShowLightMainCameraUpdates = 0;

		static public int SpriteRenderersDrawn = 0;
		static public int ShowSpriteRenderersDrawn = 0;

		static public int maskGenerations = 0;
		static public int shadowGenerations = 0;
		static public int penumbraGenerations = 0;
		static public int culled = 0;
		static public int totalLightUpdates = 0;
		
		static public TimerHelper timer;

		static public void OnGUI() {
			if (timer == null) {
				LightingDebug.timer = TimerHelper.Create();
			}
			if (timer.GetMillisecs() > 1000) {
				ShowLightBufferUpdates = LightBufferUpdates;

				LightBufferUpdates = 0;

				ShowLightMainBufferUpdates = LightMainBufferUpdates;

				LightMainBufferUpdates = 0;

				ShowLightMainCameraUpdates = LightMainCameraUpdates;

				LightMainCameraUpdates = 0;

				timer = TimerHelper.Create();

				
			}

			if (SpriteRenderersDrawn > 0) {
				ShowSpriteRenderersDrawn = SpriteRenderersDrawn;
				SpriteRenderersDrawn = 0;
			}
			

			int count = 0;
			foreach(LightingSource2D light in LightingSource2D.GetList()) {
				if (light.InCamera() == false) {
					continue;
				}
				count ++;
			}
			
			GUI.Label(new Rect(10, 10, 500, 20), "Lights in Camera: " + count + "/" + LightingSource2D.GetList().Count);
			GUI.Label(new Rect(10, 30, 500, 20), "Free Buffers: " + FBOManager.GetFreeCount() + "/" + FBOManager.GetList().Count);
			GUI.Label(new Rect(10, 50, 500, 20), "Light Buffer Updates: " + ShowLightBufferUpdates);
			GUI.Label(new Rect(10, 70, 500, 20), "Total Light Updates: " + totalLightUpdates);
			GUI.Label(new Rect(10, 90, 500, 20), "=========================");

			GUI.Label(new Rect(10, 110, 500, 20), "Mask Generations: " + maskGenerations);
			GUI.Label(new Rect(10, 130, 500, 20), "Shadow Generations: " + shadowGenerations);
			GUI.Label(new Rect(10, 150, 500, 20), "Penumbra Generations: " + penumbraGenerations);
			GUI.Label(new Rect(10, 170, 500, 20), "Objects Culled: " + culled);

			GUI.Label(new Rect(10, 190, 500, 20), "=========================");

			GUI.Label(new Rect(10, 210, 500, 20), "Sprite Renderers Drawn: " + ShowSpriteRenderersDrawn);

			GUI.Label(new Rect(10, 230, 500, 20), "=========================");
			
			GUI.Label(new Rect(10, 250, 500, 20), "Light Main Buffer Updates: " + ShowLightMainBufferUpdates);
			GUI.Label(new Rect(10, 270, 500, 20), "Light Main Camera Updates: " + ShowLightMainCameraUpdates);

			GUI.Label(new Rect(10, 290, 500, 20), "=========================");

			Texture texture = LightingMainBuffer2D.Get().bufferCamera.activeTexture;
			if (texture != null) {
				GUI.Label(new Rect(10, 310, 500, 20), "Main Buffer Resolution: " + texture.width + "x" + texture.height);
			} else {
				GUI.Label(new Rect(10, 330, 500, 20), "Main Buffer Resolution: NULL");
			}

			GUI.Label(new Rect(10, 350, 500, 20), "Glow Particles Generated: " + BlurManager.dictionary.Count);
			
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.LowerRight;
			style.normal.textColor = Color.white;
			style.fontSize = 13;

			Object[] tilemaps = Object.FindObjectsOfType(typeof(LightingTilemapCollider2D));
			Object[] lights = Object.FindObjectsOfType(typeof(LightingSource2D));
			Object[] colliders = Object.FindObjectsOfType(typeof(LightingCollider2D));
			Object[] sprites = Object.FindObjectsOfType(typeof(LightingSpriteRenderer2D));


			GUI.Label(new Rect(0, -10, Screen.width - 10, Screen.height), "Tilemap Collider Count: " + tilemaps.Length, style);
			GUI.Label(new Rect(0, -30, Screen.width - 10, Screen.height), "Lights Count: " + lights.Length, style);
			GUI.Label(new Rect(0, -50, Screen.width - 10, Screen.height), "Colliders Count: " + colliders.Length, style);
			GUI.Label(new Rect(0, -70, Screen.width - 10, Screen.height), "Sprites Count: " + sprites.Length, style);
		}
	}




	#if UNITY_EDITOR
	
	public void OnEnable() {
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	public void OnDisable() {
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}

	private void OnSceneGUI(SceneView view) {
		if (drawSceneView == false) {
			return;
		}
		
		foreach (LightingSource2D id in LightingSource2D.GetList()) {
			if (id.buffer == null) {
				continue;
			}

			if (id.isActiveAndEnabled == false) {
				continue;
			}

			if (id.buffer.bufferCamera == null) {
				continue;
			}

			if (id.buffer.material) {
				Color lightColor = id.lightColor;
				lightColor.a = id.lightAlpha / 4;
				id.buffer.material.SetColor ("_TintColor", lightColor);

				Graphics.DrawMesh(GetRenderMesh(), Matrix4x4.TRS(id.transform.position, Quaternion.Euler(0, 0, 0),  id.transform.lossyScale * id.lightSize), id.buffer.material, 0);
			}
		}
    }
	
	#endif 
}
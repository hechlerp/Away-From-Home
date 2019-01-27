using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class LightingBuffer2D : MonoBehaviour {
	public RenderTexture renderTexture;
	public int textureSize = 0;

	public Material material;
	public Material additiveMaterial;

	public LightingSource2D lightSource;
	public Camera bufferCamera;

	public bool free = true;

	// Constants
	Vector2D zero = Vector2D.Zero();
	const float uv0 = 0f;
	const float uv1 = 1f;

	public static List<LightingBuffer2D> list = new List<LightingBuffer2D>();

	public void OnEnable() {
		list.Add(this);

		//light = null;
		//free = true;
	}

	public void OnDisable() {
		list.Remove(this);
	}

	static public List<LightingBuffer2D> GetList() {
		return(list);
	}

	static public int GetCount() {
		return(list.Count);
	}

	void LateUpdate() {
		float cameraZ = -1000f;
		if (Camera.main != null) {
			cameraZ = Camera.main.transform.position.z - 10 - GetCount();
		}

		bufferCamera.transform.position = new Vector3(0, 0, cameraZ);

		transform.rotation = Quaternion.Euler(0, 0, 0);
	}

	public void OnRenderObject() {
		if(Camera.current != bufferCamera) {
			return;
		}

		Max2D.Check();

		LateUpdate ();

		// ???
		// lightSource.lightColor
		// Difference between buffer and light material?
		// material.SetColor ("_TintColor", Color.white);

		for(int layer = 0; layer < lightSource.layerCount; layer++) {
			if (lightSource.layerSetting == null || lightSource.layerSetting.Length < layer || lightSource.layerSetting.Length < layer) {
				continue;
			}

			if (lightSource.layerSetting[layer] == null) {
				continue;
			}

			int layerID = (int)lightSource.layerSetting[layer].layerID;
			if (layerID < 0) {
				continue;
			}
			
			if (lightSource.enableCollisions) {	
				switch(lightSource.layerSetting[layer].renderingOrder) {
					case LightRenderingOrder.Default:
						DrawLighting_Default(layerID);
						break;

					case LightRenderingOrder.Depth:
					case LightRenderingOrder.YAxis:
						DrawLighting_Depth(layerID);
						break;
				}
			}
		}
	
		DrawLightTexture();

		LightingManager2D.LightingDebug.LightBufferUpdates ++;
		LightingManager2D.LightingDebug.totalLightUpdates ++;
		
		// ???
		//lightSource = null;
		bufferCamera.enabled = false;
	}
	
	void DrawLighting_Default(int layer) {
		DrawShadows_Default(layer);

		DrawCollideMask_Default(layer);
	}


	public void Initiate (int textureSize) {
		SetUpRenderTexture (textureSize);
		SetUpRenderMaterial();
		SetUpCamera ();
	}

	void SetUpRenderTexture(int _textureSize) {
		textureSize = _textureSize;

		//Debug.Log("Lighting2D: New Light Texture " + textureSize + "x" + textureSize);

		renderTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);

		name = "Buffer " + GetCount() + " (size: " + textureSize + ")";
	}

	void SetUpRenderMaterial() {
		material = new Material (Shader.Find (Max2D.shaderPath + "Particles/Additive"));
		material.mainTexture = renderTexture;

		additiveMaterial = new Material (Shader.Find (Max2D.shaderPath + "Particles/Additive"));
		additiveMaterial.mainTexture = renderTexture;
	}

	void SetUpCamera() {
		bufferCamera = gameObject.AddComponent<Camera> ();
		bufferCamera.clearFlags = CameraClearFlags.Color;
		bufferCamera.backgroundColor = Color.white;
		bufferCamera.cameraType = CameraType.Game;
		bufferCamera.orthographic = true;
		bufferCamera.targetTexture = renderTexture;
		bufferCamera.farClipPlane = 0.5f;
		bufferCamera.nearClipPlane = 0f;
		bufferCamera.allowHDR = false;
		bufferCamera.allowMSAA = false;
		bufferCamera.enabled = false;
	}

	void DrawLighting_Depth(int layer) {
		List<LightingCollider2D> colliderList = LightingCollider2D.GetList();
		List<ColliderDepth> list = new List<ColliderDepth>();
		
		LightRenderingOrder order = lightSource.layerSetting[layer].renderingOrder;

		for(int id = 0; id < colliderList.Count; id++) {
			ColliderDepth depth = new ColliderDepth();
			depth.collider = colliderList[id];
			
			depth.distance = -Vector2.Distance(depth.collider.transform.position, lightSource.transform.position);
			depth.distance2 = -depth.collider.transform.position.y;

			list.Add(depth);
		}

		if (order == LightRenderingOrder.YAxis) {
			list.Sort((x, y) => x.distance2.CompareTo(y.distance2));
		} else {
			list.Sort((x, y) => x.distance.CompareTo(y.distance));
		}

		Vector2D offset = new Vector2D (-lightSource.transform.position);

		Material materialWhite = LightingManager2D.Get().whiteSpriteMaterial;
		Material materialBlack = LightingManager2D.Get().blackSpriteMaterial;

		float z = transform.position.z;
		float lightSizeSquared = Mathf.Sqrt(lightSource.lightSize * lightSource.lightSize + lightSource.lightSize * lightSource.lightSize);

		foreach (ColliderDepth id in list) {
			if ((int)id.collider.lightingCollisionLayer == layer) {	
				// Shadow
				GL.PushMatrix();
				Max2D.defaultMaterial.SetPass(0);
				GL.Begin(GL.TRIANGLES);
					GL.Color(Color.black);
					DrawShadows_ColliderShape(id.collider, lightSizeSquared, z);
				GL.End();
				GL.PopMatrix();

				// Penumbra
				GL.PushMatrix();
				LightingManager2D.Get().penumbraMaterial.SetPass(0);
				GL.Begin(GL.TRIANGLES);
					GL.Color(Color.white);
					DrawShadows_PenumbraShape(id.collider, lightSizeSquared, z);
				GL.End();
				GL.PopMatrix();
			}

			if ((int)id.collider.lightingMaskLayer == layer) {
				// Masking
				DrawColliderMask_ColliderSprite(order, id.collider, materialWhite, materialBlack, offset, z);

				GL.PushMatrix();
				materialWhite.SetPass(0);
				GL.Begin(GL.TRIANGLES);
					GL.Color(Color.white);
					DrawColliderMask_ColliderShape(id.collider, offset, z);
				GL.End();
				GL.PopMatrix();
			}
		}
	}

	void DrawCollideMask_Default(int layer) {
		float z = transform.position.z;
		Vector2D offset = new Vector2D (-lightSource.transform.position);

		Material materialWhite = LightingManager2D.Get().whiteSpriteMaterial;

		List<LightingCollider2D> colliderList = LightingCollider2D.GetList();
		List<LightingTilemapCollider2D> tilemapList = LightingTilemapCollider2D.GetList();

		if (LightingManager2D.Get().batchColliderMask) {
			if (colliderList.Count > 0) {
				materialWhite.mainTexture = colliderList[0].lightSprite.texture;

				GL.PushMatrix ();
				materialWhite.SetPass (0);
				GL.Begin (GL.QUADS);

				for(int id = 0; id < colliderList.Count; id++) {
					if ((int)colliderList[id].lightingMaskLayer != layer) {
						continue;
					}
					DrawColliderMask_ColliderSprite_Default_Batched(colliderList[id], offset, z);
				}
				
				GL.End ();
				GL.PopMatrix ();

				materialWhite.mainTexture = null;
			}

		} else {
			// For Collider Sprite Mask
			for(int id = 0; id < colliderList.Count; id++) {
				if ((int)colliderList[id].lightingMaskLayer != layer) {
					continue;
				}
				DrawColliderMask_ColliderSprite_Default(colliderList[id], materialWhite, offset, z);
			}
		}

		// Sprite
		for(int id = 0; id < tilemapList.Count; id++) {
			if ((int)tilemapList[id].lightingMaskLayer != layer) {
				continue;
			}
			DrawColliderMask_TilemapSprite(tilemapList[id], materialWhite, offset, z);
		}

		GL.PushMatrix();
        materialWhite.SetPass(0);
        GL.Begin(GL.TRIANGLES);
        GL.Color(Color.white);

		// For Collider Mask
		for(int id = 0; id < colliderList.Count; id++) {
			if ((int)colliderList[id].lightingMaskLayer != layer) {
				continue;
			}
			DrawColliderMask_ColliderShape(colliderList[id], offset, z);
		}

		for(int id = 0; id < tilemapList.Count; id++) {
			if ((int)tilemapList[id].lightingMaskLayer != layer) {
				continue;
			}
			DrawColliderMask_TilemapShape(tilemapList[id], offset, z);
		} 

		GL.End();
		GL.PopMatrix();
	}

	public int LightTilemapSize(LightingTilemapCollider2D id) {
		if (id.mapType == LightingTilemapCollider2D.MapType.SuperTilemapEditor) {
			return((int)lightSource.lightSize);
		} else {
			return((int)lightSource.lightSize + 1);
		}
	}

	public Vector2Int LightTilemapOffset(LightingTilemapCollider2D id, Vector2 scale) {
		Vector2 newPosition = lightSource.transform.position;

		newPosition -= new Vector2(id.area.position.x, id.area.position.y);
		
		newPosition.x -= id.transform.position.x;
		newPosition.y -= id.transform.position.y;
		newPosition.x -= id.cellAnchor.x;
		newPosition.y -= id.cellAnchor.y;

		newPosition.x *= scale.x;
		newPosition.y *= scale.y;
		
		if (id.mapType == LightingTilemapCollider2D.MapType.SuperTilemapEditor) {
			newPosition.x += id.area.size.x / 2;
			newPosition.y += id.area.size.y / 2;
		} else {
			newPosition.x += 1;
			newPosition.y += 1;
		}
		
		return(new Vector2Int((int)newPosition.x, (int)newPosition.y));
	}

	void DrawShadows_Default(int layer) {
		float z = transform.position.z;
		float lightSizeSquared = Mathf.Sqrt(lightSource.lightSize * lightSource.lightSize + lightSource.lightSize * lightSource.lightSize);

		List<LightingCollider2D> colliderList = LightingCollider2D.GetList();
		List<LightingTilemapCollider2D> tilemapList = LightingTilemapCollider2D.GetList();

		// Shadow Fill
		GL.PushMatrix();
		Max2D.defaultMaterial.SetPass(0);

		GL.Begin(GL.TRIANGLES);
		GL.Color(Color.black);

		for(int id = 0; id < colliderList.Count; id++) {
			if ((int)colliderList[id].lightingCollisionLayer != layer) {
				continue;
			}
			DrawShadows_ColliderMesh(colliderList[id], lightSizeSquared, z);
			DrawShadows_ColliderShape(colliderList[id], lightSizeSquared, z);
		}

		for(int id = 0; id < tilemapList.Count; id++) {
			if ((int)tilemapList[id].lightingCollisionLayer != layer) {
				continue;
			}
			DrawShadows_ColliderTilemap(tilemapList[id], lightSizeSquared, z);
		}

		GL.End();

		// Penumbra
		LightingManager2D.Get().penumbraMaterial.SetPass(0);

		GL.Begin(GL.TRIANGLES);
		GL.Color(Color.white);

		for(int id = 0; id < colliderList.Count; id++) {
			if ((int)colliderList[id].lightingCollisionLayer != layer) {
				continue;
			}
			DrawShadows_PenumbraMesh(colliderList[id], lightSizeSquared, z);
			DrawShadows_PenumbraShape(colliderList[id], lightSizeSquared, z);
		}

		for(int id = 0; id < tilemapList.Count; id++) {
			if ((int)tilemapList[id].lightingCollisionLayer != layer) {
				continue;
			}
			DrawShadows_PenumbraTilemap(tilemapList[id], lightSizeSquared, z);
		}
 
		GL.End();
		GL.PopMatrix();
	}

	///// Light Texture
	void DrawLightTexture() {
		float z = transform.position.z;
		Vector2 size = new Vector2 (bufferCamera.orthographicSize, bufferCamera.orthographicSize);

		if (lightSource.rotationEnabled) {
			Max2D.DrawImage(lightSource.GetMaterial (), Vector2.zero, size, lightSource.transform.rotation.eulerAngles.z, z);
			
			// Light Rotation!!!
			GL.PushMatrix();
			Max2D.SetColor (Color.black);
			GL.Begin(GL.TRIANGLES);
			
			Max2D.defaultMaterial.color = Color.black;
			Max2D.defaultMaterial.SetPass(0);

			float rotation = lightSource.transform.rotation.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 4;
			float squaredSize = Mathf.Sqrt((size.x * size.x) + (size.y * size.y));
			
			Vector2 p0 = Vector2D.RotToVec((double)rotation).ToVector2() * squaredSize;
			Vector2 p1 = Vector2D.RotToVec((double)rotation + Mathf.PI / 2).ToVector2() * squaredSize;
			Vector2 p2 = Vector2D.RotToVec((double)rotation + Mathf.PI).ToVector2() * squaredSize;
			Vector2 p3 = Vector2D.RotToVec((double)rotation - Mathf.PI / 2).ToVector2() * squaredSize;

			Vector2 up0 = p1 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2).ToVector2() * squaredSize;
			Vector2 up1 = p1 +  Vector2D.RotToVec((double)rotation + Mathf.PI / 4).ToVector2() * squaredSize;
			up1 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2).ToVector2() * squaredSize;

			Vector2 up2 = p0 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4).ToVector2() * squaredSize;
			up2 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2).ToVector2() * squaredSize;

			Vector2 up3 = p0 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2).ToVector2() * squaredSize;

			
			Vector2 down0 = p3 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2 - Mathf.PI ).ToVector2() * squaredSize;
			Vector2 down1 = p3 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI).ToVector2() * squaredSize;
			down1 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2 - Mathf.PI).ToVector2() * squaredSize;

			Vector2 down2 = p2 +  Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI).ToVector2() * squaredSize;
			down2 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2 - Mathf.PI).ToVector2() * squaredSize;

			Vector2 down3 = p2 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2 - Mathf.PI).ToVector2() * squaredSize;


			Vector2 left0 = p0 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2 - Mathf.PI / 2).ToVector2() * squaredSize;
			Vector2 left1 = p0 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4- Mathf.PI / 2).ToVector2() * squaredSize;
			left1 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2- Mathf.PI / 2).ToVector2() * squaredSize;

			Vector2 left2 =  p3 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4- Mathf.PI / 2).ToVector2() * squaredSize;
			left2 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2- Mathf.PI / 2).ToVector2() * squaredSize;
			
			Vector2 left3 = p3 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2- Mathf.PI / 2).ToVector2() * squaredSize;


			Vector2 right0 = p2 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2 + Mathf.PI / 2).ToVector2() * squaredSize;
			Vector2 right1 = p2 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4+ Mathf.PI / 2).ToVector2() * squaredSize;
			left1 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 + Mathf.PI / 2+ Mathf.PI / 2).ToVector2() * squaredSize;

			Vector2 right2 =  p1 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4+ Mathf.PI / 2).ToVector2() * squaredSize;
			left2 += Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2+ Mathf.PI / 2).ToVector2() * squaredSize;
			
			Vector2 right3 = p1 + Vector2D.RotToVec((double)rotation + Mathf.PI / 4 - Mathf.PI / 2+ Mathf.PI / 2).ToVector2() * squaredSize;

			Max2DMatrix.DrawTriangle(right0, right1, right2, Vector2.zero, z);
			Max2DMatrix.DrawTriangle(right2, right3, right0, Vector2.zero, z);

			Max2DMatrix.DrawTriangle(left0, left1, left2, Vector2.zero, z);
			Max2DMatrix.DrawTriangle(left2, left3, left0, Vector2.zero, z);

		
			Max2DMatrix.DrawTriangle(down0, down1, down2, Vector2.zero, z);
			Max2DMatrix.DrawTriangle(down2, down3, down0, Vector2.zero, z);

			
			Max2DMatrix.DrawTriangle(up0, up1, up2, Vector2.zero, z);
			Max2DMatrix.DrawTriangle(up2, up3, up0, Vector2.zero, z);

			GL.End();
			GL.PopMatrix();

			Max2D.defaultMaterial.color = Color.white;
		} else {
			Max2D.DrawImage(lightSource.GetMaterial (), Vector2.zero, size, 0, z);
		}
	}




	///// Draw Shadows (Penumbra) /////
	void DrawShadows_PenumbraShape(LightingCollider2D id, float lightSizeSquared, float z) {
		if (id.colliderType == LightingCollider2D.ColliderType.Mesh) {
			return;
		}

		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		if (id.colliderType == LightingCollider2D.ColliderType.None) {
			return;
		}

		List<Polygon2D> polygons = id.GetShadowCollisionPolygons();
		
		if (polygons.Count < 1) {
			return;
		}

		// Draw Inside Collider Works Fine?
		foreach(Polygon2D polygon in polygons) {
			Polygon2D poly = polygon.ToWorldSpace (id.gameObject.transform);
			poly.ToOffsetItself (new Vector2D (-lightSource.transform.position));
			
			if (lightSource.drawInsideCollider == false && poly.PointInPoly (zero)) {
				continue;
			}

			Vector2D vA, pA, vB, pB;
			float angleA, angleB;
			foreach (Pair2D p in Pair2D.GetList(poly.pointsList)) {
				vA = p.A.Copy();
				pA = p.A.Copy();

				vB = p.B.Copy();
				pB = p.B.Copy();

				angleA = (float)Vector2D.Atan2 (vA, zero);
				angleB = (float)Vector2D.Atan2 (vB, zero);

				vA.Push (angleA, lightSizeSquared);
				pA.Push (angleA - Mathf.Deg2Rad * lightSource.occlusionSize, lightSizeSquared);

				vB.Push (angleB, lightSizeSquared);
				pB.Push (angleB + Mathf.Deg2Rad * lightSource.occlusionSize, lightSizeSquared);

				GL.TexCoord2(uv0, uv0);
				GL.Vertex3((float)p.A.x,(float)p.A.y, z);
				GL.TexCoord2(uv1, uv0);
				GL.Vertex3((float)vA.x, (float)vA.y, z);
				GL.TexCoord2((float)uv0, uv1);
				GL.Vertex3((float)pA.x,(float)pA.y, z);

				GL.TexCoord2(uv0, uv0);
				GL.Vertex3((float)p.B.x,(float)p.B.y, z);
				GL.TexCoord2(uv1, uv0);
				GL.Vertex3((float)vB.x, (float)vB.y, z);
				GL.TexCoord2(uv0, uv1);
				GL.Vertex3((float)pB.x, (float)pB.y, z);
			}
			LightingManager2D.LightingDebug.penumbraGenerations ++;
		}
	}

	void DrawShadows_PenumbraMesh(LightingCollider2D id, float lightSizeSquared, float z) {
		if (id.colliderType != LightingCollider2D.ColliderType.Mesh) {
			return;
		}

		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		if (id.colliderType == LightingCollider2D.ColliderType.None) {
			return;
		}

		if (id.meshFilter == null) {
			return;
		}

		List<Polygon2D> polygons = new List<Polygon2D>();

		Mesh mesh = id.meshFilter.sharedMesh;

		Vector3 vecA, vecB, vecC;
		for (int i = 0; i <  mesh.triangles.GetLength (0); i = i + 3) {
			vecA = transform.TransformPoint(mesh.vertices [mesh.triangles [i]]);
			vecB = transform.TransformPoint(mesh.vertices [mesh.triangles [i + 1]]);
			vecC = transform.TransformPoint(mesh.vertices [mesh.triangles [i + 2]]);

			Polygon2D poly = new Polygon2D();
			poly.AddPoint(vecA.x, vecA.y);
			poly.AddPoint(vecB.x, vecB.y);
			poly.AddPoint(vecC.x, vecC.y);
			//polygons.Add(poly);
		}

		if (polygons.Count < 1) {
			return;
		}
		
		foreach(Polygon2D polygon in polygons) {
			Polygon2D poly = polygon.ToWorldSpace (id.gameObject.transform);
			poly.ToOffsetItself (new Vector2D (-lightSource.transform.position));
			
			if (poly.PointInPoly (zero)) {
				continue;
			}

			Vector2D vA, pA, vB, pB;
			float angleA, angleB;
			foreach (Pair2D p in Pair2D.GetList(poly.pointsList)) {
				vA = p.A.Copy();
				pA = p.A.Copy();

				vB = p.B.Copy();
				pB = p.B.Copy();

				angleA = (float)Vector2D.Atan2 (vA, zero);
				angleB = (float)Vector2D.Atan2 (vB, zero);

				vA.Push (angleA, lightSizeSquared);
				pA.Push (angleA - Mathf.Deg2Rad * lightSource.occlusionSize, lightSizeSquared);

				vB.Push (angleB, lightSizeSquared);
				pB.Push (angleB + Mathf.Deg2Rad * lightSource.occlusionSize, lightSizeSquared);

				GL.TexCoord2(uv0, uv0);
				GL.Vertex3((float)p.A.x,(float)p.A.y, z);
				GL.TexCoord2(uv1, uv0);
				GL.Vertex3((float)vA.x, (float)vA.y, z);
				GL.TexCoord2((float)uv0, uv1);
				GL.Vertex3((float)pA.x,(float)pA.y, z);

				GL.TexCoord2(uv0, uv0);
				GL.Vertex3((float)p.B.x,(float)p.B.y, z);
				GL.TexCoord2(uv1, uv0);
				GL.Vertex3((float)vB.x, (float)vB.y, z);
				GL.TexCoord2(uv0, uv1);
				GL.Vertex3((float)pB.x, (float)pB.y, z);
			}
			LightingManager2D.LightingDebug.penumbraGenerations ++;
		}
	}

	void DrawShadows_PenumbraTilemap(LightingTilemapCollider2D id, float lightSizeSquared, float z) {
		if (id.colliderType == LightingTilemapCollider2D.ColliderType.None) {
			return;
		}

		if (id.map == null) {
			return;
		}
		
		//Debug.Log(Mathf.Cos(id.transform.rotation.eulerAngles.x * Mathf.Deg2Rad));
		Vector3 rot = Math2D.GetPitchYawRollRad(id.transform.rotation);

		float rotationYScale = Mathf.Sin(rot.x + Mathf.PI / 2);
		float rotationXScale = Mathf.Sin(rot.y + Mathf.PI / 2);

		float scaleX = id.transform.lossyScale.x * rotationXScale * id.cellSize.x;
		float scaleY = id.transform.lossyScale.y * rotationYScale * id.cellSize.y;
		
		int sizeInt = LightTilemapSize(id);
		
		Vector2Int newPositionInt = LightTilemapOffset(id, new Vector2(scaleX, scaleY));
		
		for(int x = newPositionInt.x - sizeInt; x < newPositionInt.x + sizeInt; x++) {
			for(int y = newPositionInt.y - sizeInt; y < newPositionInt.y + sizeInt; y++) {
				if (x < 0 || y < 0) {
					continue;
				}

				if (x >= id.area.size.x || y >= id.area.size.y) {
					continue;
				}

				LightingTile tile = id.map[x, y];
				if (tile == null) {
					continue;
				}
				
				Polygon2D poly = null;
				if (id.colliderType == LightingTilemapCollider2D.ColliderType.SpriteCustomPhysicsShape) {
					if (tile.polygons.Count < 1) {
						continue;
					}
					poly = tile.polygons[0].Copy();
					poly.ToScaleItself(new Vector2(1,  1));
				} else {
					poly = Polygon2D.CreateFromRect(new Vector2(0.5f, 0.5f));
				}

				Vector2D polyOffset = Vector2D.Zero();
				polyOffset += new Vector2D(x + id.cellAnchor.x, y + id.cellAnchor.y); 
				polyOffset += new Vector2D(id.area.position.x, id.area.position.y);
				polyOffset += new Vector2D(id.transform.position.x, id.transform.position.y);

				if (id.mapType == LightingTilemapCollider2D.MapType.SuperTilemapEditor) {
					polyOffset += new Vector2D(-id.area.size.x / 2, -id.area.size.y / 2);
				}

				Vector2 pivot = Vector2.zero;
				if (tile.sprite != null) {
					pivot = tile.sprite.pivot;
					
					pivot.x /= tile.sprite.rect.width;
					pivot.y /= tile.sprite.rect.height;
					
					pivot.x -= 0.5f;
					pivot.y -= 0.5f;

					pivot.x *= scaleX * 2;
					pivot.y *= scaleY * 2;

					float pivotDist = Mathf.Sqrt(pivot.x * pivot.x + pivot.y * pivot.y);
					float pivotAngle = Mathf.Atan2(pivot.y, pivot.x);

					polyOffset.x += Mathf.Cos(pivotAngle + Mathf.PI) * pivotDist;
					polyOffset.y += Mathf.Sin(pivotAngle + Mathf.PI) * pivotDist;
				}

				polyOffset.x *= scaleX;
				polyOffset.y *= scaleY;

				if (LightingManager2D.culling && Vector2.Distance(polyOffset.ToVector2(), lightSource.transform.position) > 2 + lightSource.lightSize) {
					LightingManager2D.LightingDebug.culled ++;
					continue;
				}

				polyOffset += new Vector2D (-lightSource.transform.position);

				poly.ToOffsetItself(polyOffset);

				if (poly.PointInPoly (zero)) {
					continue;
				}

				Vector2D vA, pA, vB, pB;
				foreach (Pair2D p in Pair2D.GetList(poly.pointsList)) {
					vA = p.A.Copy();
					pA = p.A.Copy();

					vB = p.B.Copy();
					pB = p.B.Copy();

					float angleA = (float)Vector2D.Atan2 (vA, zero);
					float angleB = (float)Vector2D.Atan2 (vB, zero);

					vA.Push (angleA, lightSizeSquared);
					pA.Push (angleA - Mathf.Deg2Rad * lightSource.occlusionSize, lightSizeSquared);

					vB.Push (angleB, lightSizeSquared);
					pB.Push (angleB + Mathf.Deg2Rad * lightSource.occlusionSize, lightSizeSquared);

					GL.TexCoord2(uv0, uv0);
					GL.Vertex3((float)p.A.x,(float) p.A.y, z);
					GL.TexCoord2(uv1, uv0);
					GL.Vertex3((float)vA.x, (float)vA.y, z);
					GL.TexCoord2((float)uv0, uv1);
					GL.Vertex3((float)pA.x,(float) pA.y, z);

					GL.TexCoord2(uv0, uv0);
					GL.Vertex3((float)p.B.x,(float) p.B.y, z);
					GL.TexCoord2(uv1, uv0);
					GL.Vertex3((float)vB.x, (float)vB.y, z);
					GL.TexCoord2(uv0, uv1);
					GL.Vertex3((float)pB.x, (float)pB.y, z);
				}
				LightingManager2D.LightingDebug.penumbraGenerations ++;
			}
		}	
	}

























	///// Draw Shadows Casting /////
	
	void DrawShadows_ColliderMesh(LightingCollider2D id, float lightSizeSquared, float z) {
		if (id.colliderType != LightingCollider2D.ColliderType.Mesh) {
			return;
		}

		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		if (id.colliderType == LightingCollider2D.ColliderType.None) {
			return;
		}

		if (id.meshFilter == null) {
			return;
		}

		List<Polygon2D> polygons = new List<Polygon2D>();

		Mesh mesh = id.meshFilter.sharedMesh;

		Vector3 vecA, vecB, vecC;

		for (int i = 0; i <  mesh.triangles.GetLength (0); i = i + 3) {
			vecA = transform.TransformPoint(mesh.vertices [mesh.triangles [i]]);
			vecB = transform.TransformPoint(mesh.vertices [mesh.triangles [i + 1]]);
			vecC = transform.TransformPoint(mesh.vertices [mesh.triangles [i + 2]]);

			Polygon2D poly = new Polygon2D();
			poly.AddPoint(vecA.x, vecA.y);
			poly.AddPoint(vecB.x, vecB.y);
			poly.AddPoint(vecC.x, vecC.y);
			polygons.Add(poly);
		}

		if (polygons.Count < 1) {
			return;
		}

		Vector2D vA, vB;
		foreach(Polygon2D polygon in polygons) {
			Polygon2D poly = polygon.ToWorldSpace (id.gameObject.transform);
			poly.ToOffsetItself (new Vector2D (-lightSource.transform.position));

			if (poly.PointInPoly (zero)) {
				continue;
			}

			foreach (Pair2D p in Pair2D.GetList(poly.pointsList, false)) {
				vA = p.A.Copy();
				vB = p.B.Copy();

				vA.Push (Vector2D.Atan2 (vA, zero), lightSizeSquared);
				vB.Push (Vector2D.Atan2 (vB, zero), lightSizeSquared);
				
				Max2DMatrix.DrawTriangle(p.A ,p.B, vA, zero, z);
				Max2DMatrix.DrawTriangle(vA, vB, p.B, zero, z);
			}

			LightingManager2D.LightingDebug.shadowGenerations ++;	
		}
	}

	
	void DrawShadows_ColliderShape(LightingCollider2D id, float lightSizeSquared, float z) {
		if (id.colliderType == LightingCollider2D.ColliderType.Mesh) {
			return;
		}

		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		if (id.colliderType == LightingCollider2D.ColliderType.None) {
			return;
		}

		List<Polygon2D> polygons = id.GetShadowCollisionPolygons();

		if (polygons.Count < 1) {
			return;
		}

		Vector2D vA, vB;
		Polygon2D poly;
		switch(id.colliderType) {
			case LightingCollider2D.ColliderType.SpriteCustomPhysicsShape:
				foreach(Polygon2D polygon in polygons) {
					poly = polygon.ToWorldSpace (id.gameObject.transform);
					poly.ToOffsetItself (new Vector2D (-lightSource.transform.position));

					if (lightSource.drawInsideCollider == false && poly.PointInPoly (zero)) {
						continue;
					}
					
					foreach (Pair2D p in Pair2D.GetList(poly.pointsList, true)) {
						vA = p.A.Copy();
						vB = p.B.Copy();
						
						vA.Push (Vector2D.Atan2 (vA, zero), lightSizeSquared);
						vB.Push (Vector2D.Atan2 (vB, zero), lightSizeSquared);

						Max2DMatrix.DrawTriangle(p.A ,p.B, vA, zero, z);
						Max2DMatrix.DrawTriangle(vA, vB, p.B, zero, z);
					}

					LightingManager2D.LightingDebug.shadowGenerations ++;	
				}
				break;

			case LightingCollider2D.ColliderType.Collider:
				// Additional Code To Draw "Holes" Shadows Too
				List<Polygon2D> polygonsAndHoles = new List<Polygon2D>();
				foreach(Polygon2D polygon in polygons) {
					poly = polygon.ToWorldSpace (id.gameObject.transform);
					poly.ToOffsetItself (new Vector2D (-lightSource.transform.position));

					if (lightSource.drawInsideCollider == false && poly.PointInPoly (zero)) {
						return;
					}

					polygonsAndHoles.Add(poly);

					if (polygon.holesList.Count > 0) {
						foreach(Polygon2D hole in polygon.holesList) {
							poly = hole.ToWorldSpace (id.gameObject.transform);
							poly.ToOffsetItself (new Vector2D (-lightSource.transform.position));

							polygonsAndHoles.Add(poly);
						}
					}
				}

				polygons = polygonsAndHoles;
				
				foreach(Polygon2D polygon in polygons) {
					foreach (Pair2D p in Pair2D.GetList(polygon.pointsList, true)) {
						vA = p.A.Copy();
						vB = p.B.Copy();
						
						vA.Push (Vector2D.Atan2 (vA, zero), lightSizeSquared);
						vB.Push (Vector2D.Atan2 (vB, zero), lightSizeSquared);

						Max2DMatrix.DrawTriangle(p.A ,p.B, vA, zero, z);
						Max2DMatrix.DrawTriangle(vA, vB, p.B, zero, z);
					}

					LightingManager2D.LightingDebug.shadowGenerations ++;	
				}
				break;
		}
	}


	public void DrawShadows_ColliderTilemap(LightingTilemapCollider2D id, float lightSizeSquared, float z) {
		if (id.colliderType == LightingTilemapCollider2D.ColliderType.None) {
			return;
		}
		
		if (id.map == null) {
			return;
		}

		Vector3 rot = Math2D.GetPitchYawRollRad(id.transform.rotation);

		float rotationYScale = Mathf.Sin(rot.x + Mathf.PI / 2);
		float rotationXScale = Mathf.Sin(rot.y + Mathf.PI / 2);

		float scaleX = id.transform.lossyScale.x * rotationXScale * id.cellSize.x;
		float scaleY = id.transform.lossyScale.y * rotationYScale * id.cellSize.y;

		int sizeInt = LightTilemapSize(id);

		Vector2Int newPositionInt = LightTilemapOffset(id, new Vector2(scaleX, scaleY));
		
		Vector2D vA, vB;
		for(int x = newPositionInt.x - sizeInt; x < newPositionInt.x + sizeInt; x++) {
			for(int y = newPositionInt.y - sizeInt; y < newPositionInt.y + sizeInt; y++) {
				if (x < 0 || y < 0) {
					continue;
				}

				if (x >= id.area.size.x || y >= id.area.size.y) {
					continue;
				}
			
				LightingTile tile = id.map[x, y];
				if (tile == null) {
					continue;
				}
			
				Polygon2D poly = null;
				if (id.colliderType == LightingTilemapCollider2D.ColliderType.SpriteCustomPhysicsShape) {
					if (tile.polygons.Count < 1) {
						continue;
					}
					
					poly = tile.polygons[0].Copy();
					poly.ToScaleItself(new Vector2(1, 1));
				} else {
					poly = Polygon2D.CreateFromRect(new Vector2(0.5f, 0.5f));
				}

				Vector2D polyOffset = Vector2D.Zero();
				polyOffset += new Vector2D(x + id.cellAnchor.x, y + id.cellAnchor.y); 
				polyOffset += new Vector2D(id.area.position.x, id.area.position.y);
				polyOffset += new Vector2D(id.transform.position.x, id.transform.position.y);

				Vector2 pivot = Vector2.zero;
				if (tile.sprite != null) {
					pivot = tile.sprite.pivot;

					pivot.x /= tile.sprite.rect.width;
					pivot.y /= tile.sprite.rect.height;
					
					pivot.x -= 0.5f;
					pivot.y -= 0.5f;

					pivot.x *= scaleX * 2;
					pivot.y *= scaleY * 2;

					float pivotDist = Mathf.Sqrt(pivot.x * pivot.x + pivot.y * pivot.y);
					float pivotAngle = Mathf.Atan2(pivot.y, pivot.x);

					polyOffset.x += Mathf.Cos(pivotAngle + Mathf.PI) * pivotDist;
					polyOffset.y += Mathf.Sin(pivotAngle + Mathf.PI) * pivotDist;
				}
					
				if (id.mapType == LightingTilemapCollider2D.MapType.SuperTilemapEditor) {
					polyOffset += new Vector2D(-id.area.size.x / 2, -id.area.size.y / 2);
				}

				polyOffset.x *= scaleX;
				polyOffset.y *= scaleY;

				if (LightingManager2D.culling && Vector2.Distance(polyOffset.ToVector2(), lightSource.transform.position) > 2 + lightSource.lightSize) {
					LightingManager2D.LightingDebug.culled ++;
					continue;
				}

				polyOffset += new Vector2D (-lightSource.transform.position);
				
				poly.ToOffsetItself(polyOffset);

				if (poly.PointInPoly (zero)) {
					continue;
				}

				foreach (Pair2D p in Pair2D.GetList(poly.pointsList)) {
					vA = p.A.Copy();
					vB = p.B.Copy();
					
					vA.Push (Vector2D.Atan2 (vA, zero), lightSizeSquared);
					vB.Push (Vector2D.Atan2 (vB, zero), lightSizeSquared);
					
					Max2DMatrix.DrawTriangle(p.A ,p.B, vA, zero, z);
					Max2DMatrix.DrawTriangle(vA, vB, p.B, zero, z);
				}	
				
				LightingManager2D.LightingDebug.shadowGenerations ++;
			}
		}
	}










	///// Sprite Masking /////

	void DrawColliderMask_ColliderSprite_Default_Batched(LightingCollider2D id, Vector2D offset, float z) {
		if (id.maskType != LightingCollider2D.MaskType.Sprite) {
			return;
		}

		if (id.maskType == LightingCollider2D.MaskType.None) {
			return;
		}
		
		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		Sprite sprite = id.lightSprite;
		if (sprite == null || id.spriteRenderer == null) {
			return;
		}
		
		Vector2 p = id.transform.position;
		Vector2 scale = id.transform.lossyScale;

		Max2D.DrawSpriteBatched(id.spriteRenderer, offset.ToVector2() + p, scale, id.transform.rotation.eulerAngles.z, z);
		
		LightingManager2D.LightingDebug.maskGenerations ++;		
	}


	void DrawColliderMask_ColliderSprite_Default(LightingCollider2D id, Material material, Vector2D offset, float z) {
		if (id.maskType == LightingCollider2D.MaskType.None) {
			return;
		}
		
		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		Sprite sprite = id.lightSprite;
		if (sprite == null || id.spriteRenderer == null) {
			return;
		}

		Vector2 p = id.transform.position;
		Vector2 scale = id.transform.lossyScale;

		material.mainTexture = sprite.texture;

		Max2D.DrawSprite(material, id.spriteRenderer, offset.ToVector2() + p, scale, id.transform.rotation.eulerAngles.z, z);

		material.mainTexture = null;
		
		LightingManager2D.LightingDebug.maskGenerations ++;		
	}

	void DrawColliderMask_ColliderSprite(LightRenderingOrder lightSourceOrder, LightingCollider2D id, Material materialA, Material materialB, Vector2D offset, float z) {
		if (id.maskType == LightingCollider2D.MaskType.None) {
			return;
		}

		if (lightSourceOrder == LightRenderingOrder.YAxis && lightSource.transform.position.y > id.transform.position.y) {
			materialA = materialB;
		}
		
		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		Sprite sprite = id.lightSprite;
		if (sprite == null || id.spriteRenderer == null) {
			return;
		}

		Vector2 p = id.transform.position;
		Vector2 scale = id.transform.lossyScale;

		materialA.mainTexture = sprite.texture;

		Max2D.DrawSprite(materialA, id.spriteRenderer, offset.ToVector2() + p, scale, id.transform.rotation.eulerAngles.z, z);
		
		materialA.mainTexture = null;

		LightingManager2D.LightingDebug.maskGenerations ++;		
	}

	void DrawColliderMask_TilemapSprite(LightingTilemapCollider2D id, Material material, Vector2D offset, float z) {
		if (id.maskType != LightingTilemapCollider2D.MaskType.Sprite) {
			return;
		}
		
		if (id.map == null) {
			return;
		}

		Vector3 rot = Math2D.GetPitchYawRollRad(id.transform.rotation);

		float rotationYScale = Mathf.Sin(rot.x + Mathf.PI / 2);
		float rotationXScale = Mathf.Sin(rot.y + Mathf.PI / 2);

		float scaleX = id.transform.lossyScale.x * rotationXScale * id.cellSize.x;
		float scaleY = id.transform.lossyScale.y * rotationYScale * id.cellSize.y;

		int sizeInt = LightTilemapSize(id);

		Vector2Int newPositionInt = LightTilemapOffset(id, new Vector2(scaleX, scaleY));

		if (id.batching) {
			GL.PushMatrix ();
			material.SetPass (0); 
			GL.Begin (GL.QUADS);
			
			for(int x = newPositionInt.x - sizeInt; x < newPositionInt.x + sizeInt; x++) {
				for(int y = newPositionInt.y - sizeInt; y < newPositionInt.y + sizeInt; y++) {
					if (x < 0 || y < 0) {
						continue;
					}

					if (x >= id.area.size.x || y >= id.area.size.y) {
						continue;
					}

					LightingTile tile = id.map[x, y];
					if (tile == null) {
						continue;
					}

					if (tile.sprite == null) {
						return;
					}

					Vector2D polyOffset = Vector2D.Zero();
					polyOffset += new Vector2D(x + id.cellAnchor.x, y + id.cellAnchor.y); 
					polyOffset += new Vector2D(id.area.position.x, id.area.position.y);
					polyOffset += new Vector2D(id.transform.position.x, id.transform.position.y);

					polyOffset.x *= scaleX;
					polyOffset.y *= scaleY;

					if (LightingManager2D.culling && Vector2.Distance(polyOffset.ToVector2(), lightSource.transform.position) > 2 + lightSource.lightSize) {
						LightingManager2D.LightingDebug.culled ++;
						continue;
					}

					polyOffset += offset;

					VirtualSpriteRenderer spriteRenderer = new VirtualSpriteRenderer();
					spriteRenderer.sprite = tile.sprite;
					
					material.mainTexture = spriteRenderer.sprite.texture;

					Max2D.DrawSpriteBatched(spriteRenderer, polyOffset.ToVector2(), new Vector2(scaleX / id.cellSize.x, scaleY / id.cellSize.y), 0, z);
					
					material.mainTexture = null;

					LightingManager2D.LightingDebug.maskGenerations ++;
				}	
			}

			GL.End ();
			GL.PopMatrix ();
		} else {

			for(int x = newPositionInt.x - sizeInt; x < newPositionInt.x + sizeInt; x++) {
				for(int y = newPositionInt.y - sizeInt; y < newPositionInt.y + sizeInt; y++) {
					if (x < 0 || y < 0) {
						continue;
					}

					if (x >= id.area.size.x || y >= id.area.size.y) {
						continue;
					}

					LightingTile tile = id.map[x, y];
					if (tile == null) {
						continue;
					}

					if (tile.sprite == null) {
						return;
					}

					Vector2D polyOffset = Vector2D.Zero();
					polyOffset += new Vector2D(x + id.cellAnchor.x, y + id.cellAnchor.y); 
					polyOffset += new Vector2D(id.area.position.x, id.area.position.y);
					polyOffset += new Vector2D(id.transform.position.x, id.transform.position.y);

					polyOffset.x *= scaleX;
					polyOffset.y *= scaleY;

					if (LightingManager2D.culling && Vector2.Distance(polyOffset.ToVector2(), lightSource.transform.position) > 2 + lightSource.lightSize) {
						LightingManager2D.LightingDebug.culled ++;
						continue;
					}

					polyOffset += offset;

					VirtualSpriteRenderer spriteRenderer = new VirtualSpriteRenderer();
					spriteRenderer.sprite = tile.sprite;
					
					material.mainTexture = spriteRenderer.sprite.texture;

					Max2D.DrawSprite(material, spriteRenderer, polyOffset.ToVector2(), new Vector2(scaleX / id.cellSize.x, scaleY / id.cellSize.y), 0, z);
					
					material.mainTexture = null;

					LightingManager2D.LightingDebug.maskGenerations ++;
				}	
			}
		}
	}

	///// Shape Masking /////

	void DrawColliderMask_ColliderShape(LightingCollider2D id, Vector2D offset, float z) {
		if (id.maskType == LightingCollider2D.MaskType.Collider || id.maskType == LightingCollider2D.MaskType.SpriteCustomPhysicsShape || id.maskType == LightingCollider2D.MaskType.Mesh) {	
		} else {
			return;
		}

		if (id.maskType == LightingCollider2D.MaskType.None) {
			return;
		}
		
		if (LightingManager2D.culling && Vector2.Distance(id.transform.position, lightSource.transform.position) > id.GetCullingDistance() + lightSource.lightSize) {
			LightingManager2D.LightingDebug.culled ++;
			return;
		}

		Mesh mesh = null;
		if (id.maskType == LightingCollider2D.MaskType.Mesh) {
			if (id.meshFilter == null) {
				return;
			}
			mesh = id.meshFilter.sharedMesh;
		} else {
			mesh = id.GetShadowCollisionMesh();
		}

		if (mesh == null) {
			return;
		}

		Vector2 vecA, vecB, vecC;
		for (int i = 0; i <  mesh.triangles.GetLength (0); i = i + 3) {
			vecA = id.transform.TransformPoint(mesh.vertices [mesh.triangles [i]]);
			vecB = id.transform.TransformPoint(mesh.vertices [mesh.triangles [i + 1]]);
			vecC = id.transform.TransformPoint(mesh.vertices [mesh.triangles [i + 2]]);
			Max2DMatrix.DrawTriangle(vecA, vecB, vecC, offset.ToVector2(), z);
		}

		LightingManager2D.LightingDebug.maskGenerations ++;		
	}

	void DrawColliderMask_TilemapShape(LightingTilemapCollider2D id, Vector2D offset, float z) {
		if (id.maskType == LightingTilemapCollider2D.MaskType.SpriteCustomPhysicsShape || id.maskType == LightingTilemapCollider2D.MaskType.Rectangle) {
		} else {
			return;
		}

		if (id.map == null) {
			return;
		}

		Vector3 rot = Math2D.GetPitchYawRollRad(id.transform.rotation);

		float rotationYScale = Mathf.Sin(rot.x + Mathf.PI / 2);
		float rotationXScale = Mathf.Sin(rot.y + Mathf.PI / 2);

		float scaleX = id.transform.lossyScale.x * rotationXScale * id.cellSize.x;
		float scaleY = id.transform.lossyScale.y * rotationYScale * id.cellSize.y;

		int sizeInt = LightTilemapSize(id);

		Vector2Int newPositionInt = LightTilemapOffset(id, new Vector2(scaleX, scaleY));

		Vector2 vecA, vecB, vecC;
		for(int x = newPositionInt.x - sizeInt; x < newPositionInt.x + sizeInt; x++) {
			for(int y = newPositionInt.y - sizeInt; y < newPositionInt.y + sizeInt; y++) {
				if (x < 0 || y < 0) {
					continue;
				}

				if (x >= id.area.size.x || y >= id.area.size.y) {
					continue;
				}

				LightingTile tile = id.map[x, y];
				if (tile == null) {
					continue;
				}

				Vector2D polyOffset = Vector2D.Zero();
				polyOffset += new Vector2D(x + id.cellAnchor.x, y + id.cellAnchor.y); 
				polyOffset += new Vector2D(id.area.position.x, id.area.position.y);
				polyOffset += new Vector2D(id.transform.position.x, id.transform.position.y);
				
				if (id.mapType == LightingTilemapCollider2D.MapType.SuperTilemapEditor) {
					polyOffset += new Vector2D(-id.area.size.x / 2, -id.area.size.y / 2);
				}

				polyOffset.x *= scaleX;
				polyOffset.y *= scaleY;
				
				if (LightingManager2D.culling && Vector2.Distance(polyOffset.ToVector2(), lightSource.transform.position) > (id.cellSize.x * 2f) + lightSource.lightSize) {
					LightingManager2D.LightingDebug.culled ++;
					continue;
				}

				polyOffset += offset;

				Mesh tileMesh = null;
				if (id.maskType == LightingTilemapCollider2D.MaskType.Rectangle) {
					tileMesh = LightingTile.GetStaticTileMesh();
				} else {
					tileMesh = tile.GetTileDynamicMesh();
				}

				if (tileMesh == null) {
					continue;
				}

				for (int i = 0; i < tileMesh.triangles.GetLength (0); i = i + 3) {
					vecA = tileMesh.vertices [tileMesh.triangles [i]];
					vecB = tileMesh.vertices [tileMesh.triangles [i + 1]];
					vecC = tileMesh.vertices [tileMesh.triangles [i + 2]];
					Max2DMatrix.DrawTriangle(vecA, vecB, vecC, polyOffset.ToVector2(), z, new Vector2D(1, 1));
				}		
				LightingManager2D.LightingDebug.maskGenerations ++;				
			}
		}
	}
}
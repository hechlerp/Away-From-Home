using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingDayLighting {
	const float uv0 = 0;
	const float uv1 = 1;
	const float pi2 = Mathf.PI / 2;
    
    public static void Draw(Vector2D offset, float z) {
		float sunDirection = LightingManager2D.GetSunDirection ();
		
		// Day Soft Shadows
		GL.PushMatrix();
		Max2D.defaultMaterial.SetPass(0);
		GL.Begin(GL.TRIANGLES);
		GL.Color(Color.black);

		foreach (LightingCollider2D id in LightingCollider2D.GetList()) {
			if (id.dayHeight == false || id.height <= 0) {
				continue;
			}

			if (id.colliderType == LightingCollider2D.ColliderType.Mesh) {
				continue;
			}
			
			List<Polygon2D> polygons = null;
			switch(id.colliderType) {
				case LightingCollider2D.ColliderType.Collider:
					polygons = id.GetColliderPolygons();
					break;

				case LightingCollider2D.ColliderType.SpriteCustomPhysicsShape:
					polygons = id.GetShapePolygons();
					break;
			}

			Polygon2D poly;
			Vector3 vecA;
			Vector3 vecB;
			Vector3 vecC;

			foreach(Polygon2D polygon in polygons) {
				poly = polygon.ToWorldSpace (id.gameObject.transform);
				Polygon2D convexHull = Polygon2D.GenerateShadow(new Polygon2D(poly.pointsList), sunDirection, id.height);
				
				Mesh mesh = convexHull.CreateMesh(Vector2.zero, Vector2.zero);
				
				for (int i = 0; i <  mesh.triangles.GetLength (0); i = i + 3) {
					vecA = mesh.vertices [mesh.triangles [i]];
					vecB = mesh.vertices [mesh.triangles [i + 1]];
					vecC = mesh.vertices [mesh.triangles [i + 2]];
					Max2DMatrix.DrawTriangle(vecA.x, vecA.y, vecB.x, vecB.y, vecC.x, vecC.y, offset, z);
				}
			}
		}

		GL.End();
		GL.PopMatrix();
		
		GL.PushMatrix ();
		// Null Check?
		LightingManager2D.Get().shadowBlurMaterial.SetPass (0);
		GL.Begin (GL.TRIANGLES);
		Max2D.SetColor (Color.white);

		foreach (LightingCollider2D id in LightingCollider2D.GetList()) {
			if (id.dayHeight == false || id.height <= 0) {
				continue;
			}

			if (id.colliderType == LightingCollider2D.ColliderType.Mesh) {
				continue;
			}
			
			List<Polygon2D> polygons = null;
			switch(id.colliderType) {
				case LightingCollider2D.ColliderType.Collider:
					polygons = id.GetColliderPolygons();
					break;

				case LightingCollider2D.ColliderType.SpriteCustomPhysicsShape:
					polygons = id.GetShapePolygons();
					break;
			}

			foreach(Polygon2D polygon in polygons) {
				Polygon2D poly = polygon.ToWorldSpace (id.gameObject.transform);
				Polygon2D convexHull = Polygon2D.GenerateShadow(new Polygon2D(poly.pointsList), sunDirection, id.height);
				
				foreach (DoublePair2D p in DoublePair2D.GetList(convexHull.pointsList)) {
					Vector2D zA = new Vector2D (p.A + offset);
					Vector2D zB = new Vector2D (p.B + offset);
					Vector2D zC = zB.Copy();

					Vector2D pA = zA.Copy();
					Vector2D pB = zB.Copy();

					zA.Push (Vector2D.Atan2 (p.A, p.B) + pi2, .5f);
					zB.Push (Vector2D.Atan2 (p.A, p.B) + pi2, .5f);
					zC.Push (Vector2D.Atan2 (p.B, p.C) + pi2, .5f);
					
					GL.TexCoord2 (uv0, uv0);
					Max2D.Vertex3 (pB, z);
					GL.TexCoord2 (0.5f - uv0, uv0);
					Max2D.Vertex3 (pA, z);
					GL.TexCoord2 (0.5f - uv0, uv1);
					Max2D.Vertex3 (zA, z);
				
					GL.TexCoord2 (uv0, uv1);
					Max2D.Vertex3 (zA, z);
					GL.TexCoord2 (0.5f - uv0, uv1);
					Max2D.Vertex3 (zB, z);
					GL.TexCoord2 (0.5f - uv0, uv0);
					Max2D.Vertex3 (pB, z);
					
					GL.TexCoord2 (uv0, uv1);
					Max2D.Vertex3 (zB, z);
					GL.TexCoord2 (0.5f - uv0, uv0);
					Max2D.Vertex3 (pB, z);
					GL.TexCoord2 (0.5f - uv0, uv1);
					Max2D.Vertex3 (zC, z);
				}
			}
		}

		GL.End();
		GL.PopMatrix();

		GL.PushMatrix();
		Max2D.defaultMaterial.SetPass(0);
		GL.Begin(GL.TRIANGLES);
		GL.Color(Color.black);

		// Day Soft Shadows
		foreach (LightingTilemapCollider2D id in LightingTilemapCollider2D.GetList()) {
			if (id.map == null) {
				continue;
			}
			if (id.dayHeight == false) {
				continue;
			}
			for(int x = 0; x < id.area.size.x; x++) {
				for(int y = 0; y < id.area.size.y; y++) {
					if (id.map[x, y] == null) {
						DrawSoftShadowTile(offset + new Vector2D(x, y), z, id.height);
					}	
				}
			}	
		}

		GL.End();
		GL.PopMatrix();

		GL.PushMatrix ();
		LightingManager2D.Get().shadowBlurMaterial.SetPass (0);
		GL.Begin (GL.TRIANGLES);
		Max2D.SetColor (Color.white);
		
		// Day Soft Shadows
		foreach (LightingTilemapCollider2D id in LightingTilemapCollider2D.GetList()) {
			if (id.map == null) {
				continue;
			}
			if (id.dayHeight == false) {
				continue;
			}
			for(int x = 0; x < id.area.size.x; x++) {
				for(int y = 0; y < id.area.size.y; y++) {
					if (id.map[x, y] == null) {
						DrawSoftShadowTileBlur(offset + new Vector2D(x, y), z, id.height);
					}	
				}
			}	
		}
	
		GL.End();
		GL.PopMatrix();
		/*

		Material material = LightingManager2D.Get().whiteSpriteMaterial;
		foreach (LightingSprite2D id in LightingSprite2D.GetList()) {
			if (id.GetSpriteRenderer() == null) {
				continue;
			}
			material.mainTexture = id.GetSpriteRenderer().sprite.texture; //Debug.Log(sprite.pivot);

			Vector2 p = id.transform.position;
			Vector2 scale = id.transform.lossyScale;

			if (id.GetSpriteRenderer().flipX) {
				scale.x = -scale.x;
			}

			if (id.GetSpriteRenderer().flipY) {
				scale.y = -scale.y;
			}

			Max2D.DrawImage(material, offset.ToVector2() + p, scale, id.transform.rotation.eulerAngles.z, z);
		} */	

		float ratio = (float)Screen.width / Screen.height;
        Camera bufferCamera = LightingMainBuffer2D.Get().bufferCamera;
		Vector2 size = new Vector2(bufferCamera.orthographicSize * ratio, bufferCamera.orthographicSize);
		Vector3 pos = Camera.main.transform.position;

		Max2D.DrawImage(LightingManager2D.Get().additiveMaterial, new Vector2D(pos), new Vector2D(size), pos.z);
	
		// Day Lighting Masking
		foreach (LightingCollider2D id in LightingCollider2D.GetList()) {
			if (id.generateDayMask == false) {
				continue;
			}

			switch(id.maskType) {
				case LightingCollider2D.MaskType.SpriteCustomPhysicsShape:
					Max2D.SetColor (Color.white);
					Max2D.DrawMesh (Max2D.defaultMaterial, id.GetShapeMesh(), id.transform, offset, z);

				break;

				case LightingCollider2D.MaskType.Collider:
					Max2D.SetColor (Color.white);
					Max2D.DrawMesh (Max2D.defaultMaterial, id.GetColliderMesh(), id.transform, offset, z);

				break;

				case LightingCollider2D.MaskType.Sprite:
					if (id.spriteRenderer == null || id.spriteRenderer.sprite == null) {
						break;
					}
					
					Material material = LightingManager2D.Get().whiteSpriteMaterial;
					material.mainTexture = id.spriteRenderer.sprite.texture;

					Max2D.DrawSprite(material, id.spriteRenderer, new Vector2(id.transform.position.x, id.transform.position.y) + offset.ToVector2(), new Vector2(1, 1), id.transform.rotation.eulerAngles.z, z);

				break;
			}
		}
	}

	static void DrawSoftShadowTile(Vector2D offset, float z, float height) {
		float sunDirection = LightingManager2D.GetSunDirection ();

		Polygon2D poly = Polygon2DList.CreateFromRect(new Vector2(1, 1) / 2);
		poly = poly.ToOffset(new Vector2D(0.5f, 0.5f));

		foreach (Pair2D p in Pair2D.GetList(poly.pointsList)) {
			Vector2D vA = p.A.Copy();
			Vector2D vB = p.B.Copy();

			vA.Push (sunDirection, height);
			vB.Push (sunDirection, height);

			Max2DMatrix.DrawTriangle(p.A, p.B, vA, offset, z);
			Max2DMatrix.DrawTriangle(vA, vB, p.B, offset, z);
		}
	}

	static void DrawSoftShadowTileBlur(Vector2D offset, float z, float height) {
		float sunDirection = LightingManager2D.GetSunDirection ();

		Polygon2D poly = Polygon2DList.CreateFromRect(new Vector2(1, 1) / 2);
		offset += new Vector2D(0.5f, 0.5f);
		
		Polygon2D convexHull = Polygon2D.GenerateShadow(new Polygon2D(poly.pointsList), sunDirection, height);
		
		foreach (DoublePair2D p in DoublePair2D.GetList(convexHull.pointsList)) {
			Vector2D zA = new Vector2D (p.A + offset);
			Vector2D zB = new Vector2D (p.B + offset);
			Vector2D zC = zB.Copy();

			Vector2D pA = zA.Copy();
			Vector2D pB = zB.Copy();

			zA.Push (Vector2D.Atan2 (p.A, p.B) + pi2, .5f);
			zB.Push (Vector2D.Atan2 (p.A, p.B) + pi2, .5f);
			zC.Push (Vector2D.Atan2 (p.B, p.C) + pi2, .5f);
			
			GL.TexCoord2 (uv0, uv0);
			Max2D.Vertex3 (pB, z);
			GL.TexCoord2 (0.5f - uv0, uv0);
			Max2D.Vertex3 (pA, z);
			GL.TexCoord2 (0.5f - uv0, uv1);
			Max2D.Vertex3 (zA, z);
		
			GL.TexCoord2 (uv0, uv1);
			Max2D.Vertex3 (zA, z);
			GL.TexCoord2 (0.5f - uv0, uv1);
			Max2D.Vertex3 (zB, z);
			GL.TexCoord2 (0.5f - uv0, uv0);
			Max2D.Vertex3 (pB, z);
			
			GL.TexCoord2 (uv0, uv1);
			Max2D.Vertex3 (zB, z);
			GL.TexCoord2 (0.5f - uv0, uv0);
			Max2D.Vertex3 (pB, z);
			GL.TexCoord2 (0.5f - uv0, uv1);
			Max2D.Vertex3 (zC, z);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class LightingCollider2D : MonoBehaviour {
	public delegate void LightCollision2DEvent(LightCollision2D collision);

	public LightingLayer lightingMaskLayer = LightingLayer.Layer1;
	public LightingLayer lightingCollisionLayer = LightingLayer.Layer1;

	public event LightCollision2DEvent collisionEvents;

	public void AddEvent(LightCollision2DEvent collisionEvent) {
		collisionEvents += collisionEvent;
	}

	public void CollisionEvent(LightCollision2D collision) {
		if (collisionEvents != null) {
			collisionEvents (collision);
		}
	}

	public enum ColliderType {None, Collider, SpriteCustomPhysicsShape, Mesh};
	public enum MaskType {None, Sprite, Collider, SpriteCustomPhysicsShape, Mesh};

	public MaskType maskType = MaskType.Sprite;
	public ColliderType colliderType = ColliderType.Collider;

	public bool dayHeight = false;
	public float height = 1f;

	public bool ambientOcclusion = false;
	public bool smoothOcclusionEdges = false;
	public float occlusionSize = 1f;

	//public bool lighten = false;
	//public bool darken = false;
	//public bool lighten = false;
	//public bool darken = false;

	private List<Polygon2D> colliderPolygons = null;
	private List<Polygon2D> shapePolygons = null;

	private Mesh colliderMesh = null;
	private Mesh shapeMesh = null;

	private float colliderMeshDistance = -1f;
	private float shapeMeshDistance = -1f;

	public bool moved = false;
	public Vector2 movedPosition = Vector2.zero;
	public float movedRotation = 0;
	public Vector2 movedScale = Vector3.zero;

	public bool edgeCollider2D = false;

	public bool generateDayMask = false;

	// List<Polygon2D> collisions = new List<Polygon2D>();

	public static List<LightingCollider2D> list = new List<LightingCollider2D>();

	public Sprite lightSprite;
	public SpriteRenderer spriteRenderer;
	public MeshFilter meshFilter;

	public void OnEnable() {
		list.Add(this);

		Initialize();
	}

	public void OnDisable() {
		list.Remove(this);

		Vector2 position = movedPosition;

		float distance = GetCullingDistance();
		foreach (LightingSource2D id in LightingSource2D.GetList()) {
			if (Vector2.Distance (id.transform.position, position) < distance + id.lightSize) {
				id.update = true;
			}
		}
	}

	public List<Polygon2D> GetShadowCollisionPolygons() {
		switch(colliderType) {
			case LightingCollider2D.ColliderType.Collider:
				return(GetColliderPolygons());

			case LightingCollider2D.ColliderType.SpriteCustomPhysicsShape:
				return(GetShapePolygons());
		}
		return(null);
	}

	public Mesh GetShadowCollisionMesh() {
		switch(maskType) {
			case LightingCollider2D.MaskType.Collider:
				return(GetColliderMesh());

			case LightingCollider2D.MaskType.SpriteCustomPhysicsShape:
				return(GetShapeMesh());
		}
		return(null);
	}

	static public List<LightingCollider2D> GetList() {
		return(list);
	}

	public void Initialize() {
		movedPosition = Vector2.zero;
		movedRotation = 0;
		movedScale = Vector3.zero;
		
		colliderMeshDistance = -1f;
		shapeMeshDistance = -1;
		
		colliderPolygons = null;
		shapePolygons = null;

		GetShapePolygons();

		colliderMesh = null;
		shapeMesh = null;

		edgeCollider2D = (GetComponent<EdgeCollider2D>() != null);
		
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null) {
			lightSprite = spriteRenderer.sprite;
		}

		meshFilter = GetComponent<MeshFilter>();
		// lightMesh?
	}

	public float GetShapeDistance() {
		if (shapeMeshDistance < 0) {
			shapeMeshDistance = 0;
			if (GetShapePolygons().Count > 0) {
				foreach (Vector2D id in GetShapePolygons()[0].pointsList) {
					shapeMeshDistance = Mathf.Max(shapeMeshDistance, Vector2.Distance(id.ToVector2(), Vector2.zero));
				}
			}
		}
		return(shapeMeshDistance);
	}

	public float GetColliderDistance() {
		if (colliderMeshDistance < 0) {
			colliderMeshDistance = 0;
			if (GetColliderPolygons().Count > 0) {
				foreach (Vector2D id in GetColliderPolygons()[0].pointsList) {
					colliderMeshDistance = Mathf.Max(colliderMeshDistance, Vector2.Distance(id.ToVector2(), Vector2.zero));
				}
			}
		}
		return(colliderMeshDistance);
	}

	public void Update() {
		Vector2 position = transform.position;
		Vector2 scale = transform.lossyScale;
		float rotation = transform.rotation.eulerAngles.z;

		moved = false;

		if (movedPosition != position) {
			movedPosition = position;
			moved = true;
		}
				
		if (movedScale != scale) {
			movedScale = scale;
			moved = true;
		}

		if (movedRotation != rotation) {
			movedRotation = rotation;
			moved = true;
		}

		if (maskType == MaskType.Sprite) {
			if (spriteRenderer != null && lightSprite != spriteRenderer.sprite) {
				lightSprite = spriteRenderer.sprite;
				moved = true;
			}
		}

		if (maskType == MaskType.SpriteCustomPhysicsShape || colliderType == ColliderType.SpriteCustomPhysicsShape) {
			if (spriteRenderer != null && lightSprite != spriteRenderer.sprite) {
				lightSprite = spriteRenderer.sprite;
				moved = true;
				shapePolygons = null;
				shapeMesh = null;
				GetShapePolygons();
			}
		}

		if (moved) {
			float distance = GetCullingDistance();
		
			foreach (LightingSource2D id in LightingSource2D.GetList()) {
				if (Vector2.Distance (id.transform.position, position) < distance + id.lightSize) {
					id.update = true;
				}
			}
		}
	}

	public float GetCullingDistance() {
		switch(colliderType) {
			case LightingCollider2D.ColliderType.Collider:
				return(GetColliderDistance());
			case LightingCollider2D.ColliderType.SpriteCustomPhysicsShape:
				return(GetShapeDistance());
		}
		return(1000f);
	}

	public Mesh GetColliderMesh() {
		if (colliderMesh == null) {
			if (GetColliderPolygons().Count > 0) {
				if (GetColliderPolygons()[0].pointsList.Count > 2) {
					// Triangulate Polygon List?
					colliderMesh = PolygonTriangulator2D.Triangulate (GetColliderPolygons()[0], Vector2.zero, Vector2.zero, PolygonTriangulator2D.Triangulation.Advanced);
				}
			}
		}
		return(colliderMesh);
	}

	public List<Polygon2D> GetColliderPolygons() {
		if (colliderPolygons == null) {
			colliderPolygons = Polygon2DList.CreateFromGameObject (gameObject);
			if (colliderPolygons.Count > 0) {

			} else {
				Debug.LogWarning("SmartLighting2D: LightingCollider2D object is missing Collider2D Component");
			}
		}
		return(colliderPolygons);
	}

	public Mesh GetShapeMesh() {
		if (shapeMesh == null) {
			if (GetShapePolygons().Count > 0) {
				if (GetShapePolygons()[0].pointsList.Count > 2) {
					// Triangulate Polygon List?
					shapeMesh = PolygonTriangulator2D.Triangulate (GetShapePolygons()[0], Vector2.zero, Vector2.zero, PolygonTriangulator2D.Triangulation.Advanced);
				}
			}
		}
		return(shapeMesh);
	}

	public List<Polygon2D> GetShapePolygons() {
		if (shapePolygons == null) {
			shapePolygons = new List<Polygon2D>();

			if (lightSprite == null) {
				return(shapePolygons);
			}

			int count = lightSprite.GetPhysicsShapeCount();
			for(int i = 0; i < count; i++) {
				List<Vector2> points = new List<Vector2>();
				lightSprite.GetPhysicsShape(i, points);
				
				Polygon2D newPolygon = new Polygon2D();
				foreach(Vector2 p in points) {
					newPolygon.AddPoint(p);
				}
				shapePolygons.Add(newPolygon);
			}
		}
		return(shapePolygons);
	}
}